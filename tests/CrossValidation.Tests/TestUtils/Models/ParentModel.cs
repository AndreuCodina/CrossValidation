using System;
using System.Collections.Generic;

namespace CrossValidation.Tests.TestUtils.Models;

public class ParentModel
{
    public required string String { get; set; }
    public required string? NullableString { get; set; }
    public required DateTime? NullableDateTime { get; set; }
    public required NestedModel NestedModel { get; set; }
    public required NestedModel? NullableNestedModel { get; set; }
    public required List<int> IntList { get; set; }
    public required List<int>? NullableIntList { get; set; }
    public required List<List<int>> IntListList { get; set; }
    public required List<string> StringList { get; set; }
    public required List<string>? NullableStringList { get; set; }
    public required List<NestedModel> NestedModelList { get; set; }
    public required int Int { get; set; }
    public required int? NullableInt { get; set; }
    public required ParentModelEnum Enum { get; set; }
}