using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using Grasshopper.Kernel.Special;
using Robots;

/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class Script_Instance : GH_ScriptInstance
{
#region Utility functions
  /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
  /// <param name="text">String to print.</param>
  private void Print(string text) { /* Implementation hidden. */ }
  /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
  /// <param name="format">String format.</param>
  /// <param name="args">Formatting parameters.</param>
  private void Print(string format, params object[] args) { /* Implementation hidden. */ }
  /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj) { /* Implementation hidden. */ }
  /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj, string method_name) { /* Implementation hidden. */ }
#endregion

#region Members
  /// <summary>Gets the current Rhino document.</summary>
  private readonly RhinoDoc RhinoDocument;
  /// <summary>Gets the Grasshopper document that owns this script.</summary>
  private readonly GH_Document GrasshopperDocument;
  /// <summary>Gets the Grasshopper script component that owns this script.</summary>
  private readonly IGH_Component Component;
  /// <summary>
  /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
  /// Any subsequent call within the same solution will increment the Iteration count.
  /// </summary>
  private readonly int Iteration;
#endregion

  /// <summary>
  /// This procedure contains the user code. Input parameters are provided as regular arguments,
  /// Output parameters as ref arguments. You don't have to assign output parameters,
  /// they will have a default value.
  /// </summary>
  private void RunScript(bool AutoUpdate, string IP, ref object DataNames, ref object DataDescription, ref object RobotData)
  {
    datapack = new List<double>();

    if(FeedbackNames == null || FeedbackDesc == null)
    {
      FeedbackNames = new List<string>();
      FeedbackDesc = new List<string>();
    }
    if (RobotConnection == null)
    {
      RobotAddress = IP;
      RobotConnection = new URRealTime(RobotAddress);
      Feedbackdata = RobotConnection.FeedbackData;
    }
    if (DataAsTree == null)
    {
      DataAsTree = new DataTree<double>();
    }
    if (task == null || task.IsCompleted)
    {
      task = System.Threading.Tasks.Task.Run(() => getRobotData());
    }
    RobotData = DataAsTree;
    DataDescription = FeedbackDesc;
    DataNames = FeedbackNames;

    if (AutoUpdate == true)
    {
      Component.ExpireSolution(true);
    }
  }

  // <Custom additional code> 
  System.Threading.Tasks.Task task = null;
  URRealTime RobotConnection;
  String RobotAddress;
  DataTree<double> DataAsTree;
  List<double> datapack;
  FeedbackType[] Feedbackdata;
  List<string> FeedbackNames;
  List<string> FeedbackDesc;
  GH_Path path;

  void getRobotData()
  {
    RobotConnection.UpdateFeedback();
    Feedbackdata = RobotConnection.FeedbackData;

    for (var j = 0; j < Feedbackdata.Length; j++)
    {
      if (FeedbackNames.Count > j)
      {
        FeedbackNames[j] = Feedbackdata[j].Meaning;
        FeedbackDesc[j] = Feedbackdata[j].Notes;
      }
      else
      {
        FeedbackNames.Add(Feedbackdata[j].Meaning);
        FeedbackDesc.Add(Feedbackdata[j].Notes);
      }
      datapack = new List<double>();
      for (var k = 0; k < Feedbackdata[j].Value.Length; k++)
      {
        datapack.Add(Feedbackdata[j].Value[k]);
      }
      path = new GH_Path(j);
      DataAsTree.RemovePath(j);
      DataAsTree.AddRange(datapack, path);
      DataAsTree.TrimExcess();

    }
  }

  // </Custom additional code> 
}
