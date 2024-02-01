# Edge-RuleEngine
A rule engine based on C# Dot net and Json

Example Rule:
[
  {
    "WorkflowName": "Discount",
    "Rules": [
      {
        "RuleName": "GiveDiscount10",
        "SuccessEvent": "10",
        "ErrorMessage": "One or more adjust rules failed.",
        "ErrorType": "Error",
        "RuleExpressionType": "LambdaExpression",
        "Expression": "input1.country == \"india\" AND input1.loyaltyFactor <= 2 AND input1.totalPurchasesToDate >= 5000"
      },
      {
        "RuleName": "GiveDiscount20",
        "SuccessEvent": "20",
        "ErrorMessage": "One or more adjust rules failed.",
        "ErrorType": "Error",
        "RuleExpressionType": "LambdaExpression",
        "Expression": "input1.country == \"india\" AND input1.loyaltyFactor >= 3 AND input1.totalPurchasesToDate >= 10000"
      }
    ]
  }
]

Example Code in c#:

List<Rule> rules = new List<Rule>();

Rule rule = new Rule();
rule.RuleName = "Test Rule";
rule.SuccessEvent = "Count is within tolerance.";
rule.ErrorMessage = "Over expected.";
rule.Expression = "count < 3";
rule.RuleExpressionType = RuleExpressionType.LambdaExpression;
rules.Add(rule);

var workflows = new List<Workflow>();

Workflow exampleWorkflow = new Workflow();
exampleWorkflow.WorkflowName = "Example Workflow";
exampleWorkflow.Rules = rules;

workflows.Add(exampleWorkflow);

var bre = new RulesEngine.RulesEngine(workflows.ToArray());
