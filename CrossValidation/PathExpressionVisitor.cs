using System.Linq.Expressions;

namespace CrossValidation;

public class PathExpressionVisitor : ExpressionVisitor
{
    private readonly List<string> _parts = new();

    public string FieldFullPath { get; private set; } = "";
    public string FieldName { get; private set; } = "";

    private PathExpressionVisitor()
    {
    }

    public static PathExpressionVisitor Create(Expression expression)
    {
        var visitor = new PathExpressionVisitor();
        visitor.ExtractPath(expression);
        return visitor;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        _parts.Add(node.Member.Name);
        return base.VisitMember(node);
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        _parts.Add(node.Name!);
        return base.VisitParameter(node);
    }

    public void ExtractPath(Expression expression)
    {
        var expressionToVisit = expression is LambdaExpression lambdaExpression
            ? lambdaExpression.Body
            : expression;
        Visit(expressionToVisit);
        _parts.Reverse();
        var partsWithLambdaFieldSkipped = _parts.Skip(1).ToList();
        FieldFullPath = string.Join(".", partsWithLambdaFieldSkipped);
        FieldName = partsWithLambdaFieldSkipped.LastOrDefault() ?? "";
    }
}