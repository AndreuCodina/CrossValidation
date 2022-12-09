using System;
using System.Collections.Generic;

namespace CrossValidationTests.Models;

public class CreateOrderModel
{
    public string? Coupon { get; set; }
    public DateTime? DeliveryTime { get; set; }
    public CreateOrderModelDeliveryAddress DeliveryAddress { get; set; }
    public IEnumerable<CreateOrderModelProduct> Products { get; set; }
    public IEnumerable<int> ColorIds { get; set; }
}

public class CreateOrderModelDeliveryAddress
{
    public string Street { get; set; }
    public int Number { get; set; }
    public CreateOrderModelAddressInformation Information { get; set; }
}
    
public class CreateOrderModelAddressInformation
{
    public bool IsGood { get; set; }
}

public class CreateOrderModelProduct
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public IEnumerable<CreateOrderModelProductColor> Colors { get; set; }
    public IEnumerable<int> SizeIds { get; set; }
}
    
public enum CreateOrderModelProductColor
{
    Red = 1,
    Blue = 2
}