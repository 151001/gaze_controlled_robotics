using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GOTSDK.Position;
using System.Xml.Linq;
using GOTSDK;
using System.Windows.Media.Media3D;

namespace Robot
{   
	/// <summary>
	/// An example of how to persist a Scenario3D instance. Essentially:
	/// 1) The size of the calibration triangle (in case of the calibrator, that would be e.g. 750 mm for each side in the triangle).
	/// 2) The measured distances from each receiver to each corner of the calibration triangle.
	/// 3) Any merge data (ScenarioTranslation, ScenarioRotationAxis, ScenarioRotationAngle)
	/// </summary>
	public static class Scenario3DPersistence
	{		
		public static XDocument Save(IEnumerable<Scenario3D> scenarios)
		{			
			var root = new XElement("Calibration");
			var xmlDoc = new XDocument(root);

			foreach (var scenario in scenarios)
			{
				var scenarioNode = new XElement("Scenario3D", new XAttribute("Main", scenario.IsMainScenario));

				// Get the size of the calibration triangle (in case of the auto-calibrator, this will usually be the same length for all sides)
				scenarioNode.Add(new XElement("CalibrationTriangle",
							new XAttribute("From00toX0", scenario.TriangleSize00toX0),
							new XAttribute("FromX0toXY", scenario.TriangleSizeX0toXY),
							new XAttribute("FromXYto00", scenario.TriangleSizeXYto00)));

				foreach (var receiver in scenario.Receivers)
				{
					// Get the distance from the receiver to each of the three corners of the calibration triangle
					scenarioNode.Add(new XElement("Receiver",
								new XAttribute("Id", receiver.Value.ToString()),
								new XAttribute("DistanceTo00", (int)scenario.GetCalibrationDistance(receiver, Calibration3DPosition.Pos00)),
								new XAttribute("DistanceToX0", (int)scenario.GetCalibrationDistance(receiver, Calibration3DPosition.PosX0)),
								new XAttribute("DistanceToXY", (int)scenario.GetCalibrationDistance(receiver, Calibration3DPosition.PosXY))));
				}

				if (!scenario.IsMainScenario)
				{
					scenarioNode.Add(new XElement("MergeTranslation",
										new XAttribute("X", (int)scenario.ScenarioTranslation.X),
										new XAttribute("Y", (int)scenario.ScenarioTranslation.Y),
										new XAttribute("Z", (int)scenario.ScenarioTranslation.Z)));

					scenarioNode.Add(new XElement("MergeRotationAxis",
										new XAttribute("X", (int)scenario.ScenarioRotationAxis.X),
										new XAttribute("Y", (int)scenario.ScenarioRotationAxis.Y),
										new XAttribute("Z", (int)scenario.ScenarioRotationAxis.Z)));

					scenarioNode.Add(new XElement("MergeRotationAngle", new XAttribute("Degrees", (int)scenario.ScenarioRotationAngle)));				
				}
				
				root.Add(scenarioNode);
			}

			return xmlDoc;
		}

		public static Scenario3D[] Load(XDocument doc)
		{
			var result = new List<Scenario3D>();

			// Assume the XML is well-formatted for now. Error handling should be added.
			foreach (var scenarioNode in doc.Descendants("Scenario3D"))
			{
				var scenario = new Scenario3D();
				var calibrationNode = scenarioNode.Descendants("CalibrationTriangle").First();

				// Read size of calibration triangle
				scenario.TriangleSize00toX0 = int.Parse(calibrationNode.Attribute("From00toX0").Value);
				scenario.TriangleSizeX0toXY = int.Parse(calibrationNode.Attribute("FromX0toXY").Value);
				scenario.TriangleSizeXYto00 = int.Parse(calibrationNode.Attribute("FromXYto00").Value);

				// Load receivers and their distances to the calibration triangle
				foreach (var receiverNode in scenarioNode.Descendants("Receiver"))
				{
					var receiverAddress = new GOTAddress(int.Parse(receiverNode.Attribute("Id").Value));
					scenario.AddReceiver(receiverAddress);
					scenario.SetCalibrationDistance(receiverAddress, Calibration3DPosition.Pos00, int.Parse(receiverNode.Attribute("DistanceTo00").Value));
					scenario.SetCalibrationDistance(receiverAddress, Calibration3DPosition.PosX0, int.Parse(receiverNode.Attribute("DistanceToX0").Value));
					scenario.SetCalibrationDistance(receiverAddress, Calibration3DPosition.PosXY, int.Parse(receiverNode.Attribute("DistanceToXY").Value));
				}

				// Load merge information
				var mergeTranslation = scenarioNode.Descendants("MergeTranslation").FirstOrDefault();
				if (mergeTranslation != null)
					scenario.ScenarioTranslation = new Vector3D(int.Parse(mergeTranslation.Attribute("X").Value), int.Parse(mergeTranslation.Attribute("Y").Value), int.Parse(mergeTranslation.Attribute("Z").Value));

				var mergeRotationAxis = scenarioNode.Descendants("MergeRotationAxis").FirstOrDefault();
				if (mergeRotationAxis != null)
					scenario.ScenarioRotationAxis = new Vector3D(int.Parse(mergeRotationAxis.Attribute("X").Value), int.Parse(mergeRotationAxis.Attribute("Y").Value), int.Parse(mergeRotationAxis.Attribute("Z").Value));

				var mergeRotationAngle = scenarioNode.Descendants("MergeRotationAngle").FirstOrDefault();
				if (mergeRotationAngle != null)
					scenario.ScenarioRotationAngle = int.Parse(mergeRotationAngle.Attribute("Degrees").Value);
				
				scenario.IsMainScenario = scenarioNode.Attribute("Main") != null && scenarioNode.Attribute("Main").Value.ToLower() == "true";

				// Important! Always call UpdateConfigurations after setting all values.
				Scenario3D.ScenarioStatus status = scenario.UpdateConfigurations();

				// TODO: status should also be checked, in case invalid data was saved.

				result.Add(scenario);
			}

			return result.ToArray();
		}
	}
}
