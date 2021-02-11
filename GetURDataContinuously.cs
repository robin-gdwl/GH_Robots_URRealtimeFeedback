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
  private void RunScript(bool AutoUpdate, string IP, ref object RobotData)
  {
    datapack = new List<double>();

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
      task = System.Threading.Tasks.Task.Run(() => Animation());
    }
    RobotData = DataAsTree;
    
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
  List<FeedbackType> Feedbackdata;
  GH_Path path;

  void Animation()
  {
    RobotConnection.UpdateFeedback();
    Feedbackdata = RobotConnection.FeedbackData;

    for (var j = 0; j < Feedbackdata.Count; j++)
    {
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


  /// <summary>
  /// This method will be called once every solution, before any calls to RunScript.
  /// </summary>
  public override void BeforeRunScript()
  { }
  /// <summary>
  /// This method will be called once every solution, after any calls to RunScript.
  /// </summary>
  public override void AfterRunScript()
  { }

  // </Custom additional code> 
}