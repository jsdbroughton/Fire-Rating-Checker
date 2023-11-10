using Objects;
using Objects.BuiltElements.Revit;
using Speckle.Automate.Sdk;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using Speckle.Core.Models.Extensions;
using Speckle.Core.Models.GraphTraversal;
using System;

static class AutomateFunction
{
  public static async Task Run(
    AutomationContext automationContext,
    FunctionInputs functionInputs
  )
  {
    Console.WriteLine("Starting execution");
    _ = typeof(ObjectsKit).Assembly; // INFO: Force objects kit to initialize

    Console.WriteLine("Receiving version");
    var commitObject = await automationContext.ReceiveVersion();

    Console.WriteLine("Received version: " + commitObject);
    var objects = commitObject.Flatten().Where(x => x["parameters"] != null);

    //just forcing the nuget to load
    //Autodesk.Revit.DB.XYZ x = new Autodesk.Revit.DB.XYZ();
    //var converter = new ConverterRevit();

    //var traverseFunction = DefaultTraversal.CreateRevitTraversalFunc(converter);
    //var objectsToConvert = traverseFunction
    //  .Traverse(commitObject)
    //  .Select(tc => tc.current)
    //  .ToList();

    int failedElements = 0;

    foreach (var item in objects)
    {

      Console.WriteLine($"Processing id={item.id} name={item["name"]} cat={item["category"]}");

      var parameters = item["parameters"] as Base;
      if (parameters == null)
      {
        Console.WriteLine($"No parameters found");
        continue;
      }

      var p = parameters["Fire Rating"] as Parameter;
      if (p == null || p.value == null)
      {
        Console.WriteLine($"No Fire Rating found");
        continue;
      }


      int fireRating;

      if (!int.TryParse(p.value.ToString(), out fireRating))
      {
        Console.WriteLine($"No valid Fire Rating found");
        continue;
      }

      Console.WriteLine($"Fire rating for {item.id} is {fireRating}");

      if (fireRating < functionInputs.MinFireRating)
      {
        failedElements++;
        automationContext.AttachErrorToObjects("Fire 🔥", new List<string> { item.id });
      }


    }

    if (failedElements > 0)
      automationContext.MarkRunFailed($"{failedElements} elements failed Fire Rating check");
    else
      automationContext.MarkRunSuccess($"Check complete");

  }
}
