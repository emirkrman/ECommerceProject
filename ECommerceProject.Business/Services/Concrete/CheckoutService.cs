using ECommerceProject.Business.Models.Checkout;
using ECommerceProject.Business.Models.Payments;
using ECommerceProject.Business.Services.Abstract;
using ECommerceProject.Data.Context;
using ECommerceProject.Entity.Common;
using ECommerceProject.Entity.Concrete;
using Microsoft.EntityFrameworkCore;

namespace ECommerceProject.Business.Services.Concrete;

public class CheckoutService : ICheckoutService
{
    private readonly AppDbContext _context;
    private readonly ICartService _cartService;
    private readonly IPaymentService _paymentService;
    private readonly IStockReservationService _stockReservationService;

    public CheckoutService(
        AppDbContext context,
        ICartService cartService,
        IPaymentService paymentService,
        IStockReservationService stockReservationService)
    {
        _context = context;
        _cartService = cartService;
        _paymentService = paymentService;
        _stockReservationService = stockReservationService;
    }

    public async Task<CheckoutSummaryModel> GetCheckoutSummaryAsync(int userId)
    {
        var cart = await _cartService.GetUserCartAsync(userId);
        var addresses = await _context.UserAddresses
            .AsNoTracking()
            .Where(address => address.UserId == userId)
            .OrderByDescending(address => address.Id)
            .ToListAsync();
        var cards = await _context.UserPaymentCards
            .AsNoTracking()
            .Where(card => card.UserId == userId)
            .OrderByDescending(card => card.Id)
            .ToListAsync();

        return new CheckoutSummaryModel
        {
            Items = cart?.Items
                .Where(item => item.Product != null)
                .Select(item => new CheckoutItemModel
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product!.Name,
                    ImageUrl = item.Product.ImageUrl,
                    UnitPrice = item.Product.Price,
                    Quantity = item.Quantity
                })
                .ToList() ?? new List<CheckoutItemModel>(),
            SavedAddresses = addresses.Select(MapAddress).ToList(),
            SavedCards = cards.Select(MapCard).ToList()
        };
    }

    public async Task<CheckoutResult> PlaceOrderAsync(int userId, CheckoutRequest request)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || !cart.Items.Any())
            return CheckoutResult.Failure("Sepetiniz boş.");

        var unavailableItem = cart.Items.FirstOrDefault(item =>
            item.Product == null ||
            !item.Product.IsActive ||
            item.Product.Stock < item.Quantity);

        if (unavailableItem != null)
            return CheckoutResult.Failure("Sepette stokta olmayan veya yetersiz stoklu ürün var.");

        var totalAmount = cart.Items.Sum(item => item.Product!.Price * item.Quantity);
        var productIds = cart.Items.Select(item => item.ProductId).ToList();
        var paymentCard = await GetPaymentCardAsync(userId, request);

        if (paymentCard == null)
            return CheckoutResult.Failure("Kart bilgileri bulunamadı.");

        var paymentResult = await _paymentService.PayAsync(new PaymentRequest
        {
            Amount = totalAmount,
            CardHolderName = paymentCard.CardHolderName,
            CardNumber = paymentCard.CardNumber,
            ExpiryMonth = paymentCard.ExpiryMonth,
            ExpiryYear = paymentCard.ExpiryYear,
            Cvv = paymentCard.Cvv
        });

        if (!paymentResult.Succeeded)
            return CheckoutResult.Failure(paymentResult.Message);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var address = await GetOrCreateAddressAsync(userId, request);
        var order = CreateOrder(userId, address, cart.Items, totalAmount, paymentResult, paymentCard.CardNumber);

        await _context.Orders.AddAsync(order);

        foreach (var cartItem in cart.Items)
        {
            cartItem.Product!.Stock -= cartItem.Quantity;
            cartItem.Product.UpdatedDate = DateTime.UtcNow;
        }

        _context.Carts.Remove(cart);

        await SavePaymentCardIfRequestedAsync(userId, request, paymentCard);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        await _stockReservationService.ReleaseManyAsync(userId, productIds);

        return CheckoutResult.Success(order.Id, "Siparişiniz başarıyla oluşturuldu.");
    }

    private async Task<UserAddress> GetOrCreateAddressAsync(int userId, CheckoutRequest request)
    {
        if (request.SelectedAddressId.HasValue)
        {
            var existingAddress = await _context.UserAddresses
                .FirstOrDefaultAsync(address =>
                    address.Id == request.SelectedAddressId.Value &&
                    address.UserId == userId);

            if (existingAddress != null)
                return existingAddress;
        }

        var address = new UserAddress
        {
            UserId = userId,
            Title = Normalize(request.AddressTitle, "Adres"),
            FullName = Normalize(request.FullName, "Ad Soyad"),
            PhoneNumber = Normalize(request.PhoneNumber, "Telefon"),
            City = Normalize(request.City, "Şehir"),
            District = Normalize(request.District, "İlçe"),
            AddressLine = Normalize(request.AddressLine, "Adres detayı"),
            CreatedDate = DateTime.UtcNow
        };

        await _context.UserAddresses.AddAsync(address);
        return address;
    }

    private async Task<PaymentCardDetails?> GetPaymentCardAsync(int userId, CheckoutRequest request)
    {
        if (request.SelectedCardId.HasValue)
        {
            var savedCard = await _context.UserPaymentCards
                .AsNoTracking()
                .FirstOrDefaultAsync(card =>
                    card.Id == request.SelectedCardId.Value &&
                    card.UserId == userId);

            return savedCard == null
                ? null
                : new PaymentCardDetails(
                    savedCard.CardHolderName,
                    savedCard.CardNumber,
                    savedCard.ExpiryMonth,
                    savedCard.ExpiryYear,
                    savedCard.Cvv);
        }

        return new PaymentCardDetails(
            Normalize(request.CardHolderName, "Kart Sahibi"),
            FormatCardNumber(request.CardNumber),
            Normalize(request.ExpiryMonth, "01"),
            Normalize(request.ExpiryYear, DateTime.UtcNow.Year.ToString()),
            Normalize(request.Cvv, "000"));
    }

    private async Task SavePaymentCardIfRequestedAsync(
        int userId,
        CheckoutRequest request,
        PaymentCardDetails paymentCard)
    {
        if (!request.SaveCard || request.SelectedCardId.HasValue)
            return;

        var cardExists = await _context.UserPaymentCards.AnyAsync(card =>
            card.UserId == userId &&
            card.CardNumber == paymentCard.CardNumber &&
            card.ExpiryMonth == paymentCard.ExpiryMonth &&
            card.ExpiryYear == paymentCard.ExpiryYear);

        if (cardExists)
            return;

        await _context.UserPaymentCards.AddAsync(new UserPaymentCard
        {
            UserId = userId,
            CardHolderName = paymentCard.CardHolderName,
            CardNumber = paymentCard.CardNumber,
            ExpiryMonth = paymentCard.ExpiryMonth,
            ExpiryYear = paymentCard.ExpiryYear,
            Cvv = paymentCard.Cvv,
            CardLastFour = GetCardLastFour(paymentCard.CardNumber) ?? string.Empty,
            CreatedDate = DateTime.UtcNow
        });
    }

    private static Order CreateOrder(
        int userId,
        UserAddress address,
        IEnumerable<CartItem> cartItems,
        decimal totalAmount,
        PaymentResult paymentResult,
        string cardNumber)
    {
        return new Order
        {
            UserId = userId,
            UserAddress = address,
            Status = OrderStatus.PaymentSucceeded,
            TotalAmount = totalAmount,
            PaymentMessage = paymentResult.Message,
            PaymentCardLastFour = GetCardLastFour(cardNumber),
            CreatedDate = DateTime.UtcNow,
            Items = cartItems.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.Product!.Name,
                UnitPrice = item.Product.Price,
                Quantity = item.Quantity,
                CreatedDate = DateTime.UtcNow
            }).ToList()
        };
    }

    private static CheckoutAddressModel MapAddress(UserAddress address)
    {
        return new CheckoutAddressModel
        {
            Id = address.Id,
            Title = address.Title,
            FullName = address.FullName,
            PhoneNumber = address.PhoneNumber,
            City = address.City,
            District = address.District,
            AddressLine = address.AddressLine
        };
    }

    private static CheckoutCardModel MapCard(UserPaymentCard card)
    {
        return new CheckoutCardModel
        {
            Id = card.Id,
            CardHolderName = card.CardHolderName,
            CardLastFour = card.CardLastFour
        };
    }

    private static string Normalize(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private static string FormatCardNumber(string? cardNumber)
    {
        var digits = GetCardDigits(cardNumber);
        return string.Join("-", digits.Chunk(4).Select(chunk => new string(chunk)));
    }

    private static string? GetCardLastFour(string cardNumber)
    {
        var digits = GetCardDigits(cardNumber);
        return digits.Length < 4 ? null : digits[^4..];
    }

    private static string GetCardDigits(string? cardNumber)
    {
        return new string((cardNumber ?? string.Empty).Where(char.IsDigit).ToArray());
    }

    private sealed record PaymentCardDetails(
        string CardHolderName,
        string CardNumber,
        string ExpiryMonth,
        string ExpiryYear,
        string Cvv);
}
