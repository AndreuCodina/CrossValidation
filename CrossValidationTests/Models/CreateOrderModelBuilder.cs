using System;
using System.Collections.Generic;

namespace CrossValidationTests.Models;

public class CreateOrderModelBuilder
{
    private string? _coupon = null;
    private CreateOrderModelDeliveryAddress _deliveryAddress = new DeliveryAddressBuilder().Build();
    private IEnumerable<int>? _colorIds = null;

    public CreateOrderModel Build()
    {
        var model = new CreateOrderModel
        {
            Coupon = _coupon,
            DeliveryTime = DateTime.UtcNow,
            DeliveryAddress = _deliveryAddress,
            Products = new List<CreateOrderModelProduct>
            {
                new CreateOrderModelProduct
                {
                    ProductId = 1,
                    Quantity = 1,
                    Colors = new List<CreateOrderModelProductColor>
                    {
                        CreateOrderModelProductColor.Blue
                    },
                    SizeIds = new List<int>
                    {
                        1
                    }
                }
            },
            ColorIds = _colorIds
        };

        return model;
    }

    public CreateOrderModelBuilder WithDeliveryAddress(Action<DeliveryAddressBuilder> builder)
    {
        var dab = new DeliveryAddressBuilder();
        builder(dab);
        _deliveryAddress = dab.Build();
        return this;
    }

    public CreateOrderModelBuilder WithCoupon(string? coupon)
    {
        _coupon = coupon;
        return this;
    }
    
    public CreateOrderModelBuilder WithColorIds(IEnumerable<int>? colorIds)
    {
        _colorIds = colorIds;
        return this;
    }

    public class DeliveryAddressBuilder
    {
        public CreateOrderModelDeliveryAddress Build()
        {
            return new CreateOrderModelDeliveryAddress
            {
                Street = "Genova",
                Number = 1,
                Information = new CreateOrderModelAddressInformation
                {
                    IsGood = true
                }
            };
        }
    }
}