using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreationModelPlugin
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreationModel : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {         

            CreatWall(commandData);                    


            return Result.Succeeded;
        }
        public Level GetLevel1(ExternalCommandData commandData)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            List<Level> listLevel = new FilteredElementCollector(doc)
                  .OfClass(typeof(Level))
                  .OfType<Level>()
                  .ToList();
            var level1 = listLevel
                .Where(l => l.Name.Equals("Уровень 1"))
                .FirstOrDefault();
            
            return level1;
        }
        public Level GetLevel2(ExternalCommandData commandData)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            List<Level> listLevel = new FilteredElementCollector(doc)
                  .OfClass(typeof(Level))
                  .OfType<Level>()
                  .ToList();
            
            var level2 = listLevel
           .Where(l => l.Name.Equals("Уровень 2"))
           .FirstOrDefault();
            return level2;
        }
        public void CreatWall(ExternalCommandData commandData)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            double width = UnitUtils.ConvertToInternalUnits(10000, UnitTypeId.Millimeters);
            double depth = UnitUtils.ConvertToInternalUnits(5000, UnitTypeId.Millimeters);
            double dx = width / 2;
            double dy = depth / 2;

            List<XYZ> points = new List<XYZ>();
            points.Add(new XYZ(-dx, -dy, 0));
            points.Add(new XYZ(dx, -dy, 0));
            points.Add(new XYZ(dx, dy, 0));
            points.Add(new XYZ(-dx, dy, 0));
            points.Add(new XYZ(-dx, -dy, 0));

            List<Wall> walls = new List<Wall>();
            using (Transaction ts = new Transaction(doc, "Set wall"))
            {
                ts.Start();
                for (int i = 0; i < 4; i++)
                {
                    Line line = Line.CreateBound(points[i], points[i + 1]);
                    Wall wall = Wall.Create(doc, line, GetLevel1(commandData).Id, false);
                    walls.Add(wall);
                    wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(GetLevel2(commandData).Id);
                }
             
                ts.Commit();                
            }
            

        }
    }
}
