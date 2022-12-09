using System;
using System.Collections.Generic;

namespace CrossValidationTests.Models;

public class CreateOrderModel
{
    public required string? Coupon { get; set; }
    public required DateTime? DeliveryTime { get; set; }
    public required CreateOrderModelDeliveryAddress DeliveryAddress { get; set; }
    public required IEnumerable<CreateOrderModelProduct> Products { get; set; }
    public required List<int> ColorIds { get; set; }
}

public class CreateOrderModelDeliveryAddress
{
    public required string Street { get; set; }
    public required int Number { get; set; }
    public required CreateOrderModelAddressInformation Information { get; set; }
}
    
public class CreateOrderModelAddressInformation
{
    public required bool IsGood { get; set; }
}

public class CreateOrderModelProduct
{
    public required int ProductId { get; set; }
    public required int Quantity { get; set; }
    public required IEnumerable<CreateOrderModelProductColor> Colors { get; set; }
    public required IEnumerable<int> SizeIds { get; set; }
}
    
public enum CreateOrderModelProductColor
{
    Red = 1,
    Blue = 2
}