using AutoMapper;
using ECommerceProject.Business.Models.Accounts;
using ECommerceProject.Business.Models.Products;
using ECommerceProject.Web.ViewModels.Account;
using ECommerceProject.Web.ViewModels.Products;

namespace ECommerceProject.Web.Mapping;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<RegisterViewModel, RegisterRequest>();
        CreateMap<LoginViewModel, LoginRequest>();

        CreateMap<ProductListResult, ProductListViewModel>()
            .ForMember(destination => destination.ListedProducts,
                options => options.MapFrom(source => source.Products));

        CreateMap<ProductDetailsResult, ProductDetailsViewModel>();
        CreateMap<ProductFormViewModel, ProductFormData>();
        CreateMap<ProductFormData, ProductFormViewModel>();
    }
}
