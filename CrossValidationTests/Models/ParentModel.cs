using System;
using System.Collections.Generic;

namespace CrossValidationTests.Models;

public class ParentModel
{
    public required string? NullableString { get; set; }
    public required DateTime? NullableDateTime { get; set; }
    public required NestedModel NestedModel { get; set; }
    public required List<int>? NullableIntList { get; set; }
    public required int? NullableInt { get; set; }
    public required NestedEnum NestedEnum { get; set; }
}