﻿using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
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
            Document doc = commandData.Application.ActiveUIDocument.Document;
            CreatWall(doc);


            return Result.Succeeded;
        }
        public Level GetLevel1(Document doc)
        {

            List<Level> listLevel = new FilteredElementCollector(doc)
                  .OfClass(typeof(Level))
                  .OfType<Level>()
                  .ToList();
            var level1 = listLevel
                .Where(l => l.Name.Equals("Уровень 1"))
                .FirstOrDefault();

            return level1;
        }
        public Level GetLevel2(Document doc)
        {
            List<Level> listLevel = new FilteredElementCollector(doc)
                  .OfClass(typeof(Level))
                  .OfType<Level>()
                  .ToList();

            var level2 = listLevel
           .Where(l => l.Name.Equals("Уровень 2"))
           .FirstOrDefault();
            return level2;
        }
        public void CreatWall(Document doc)
        {
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
                    Wall wall = Wall.Create(doc, line, GetLevel1(doc).Id, false);
                    walls.Add(wall);
                    wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(GetLevel2(doc).Id);
                }
                AddDoors(doc, walls[0]);
                AddWindows(doc, walls);
                ts.Commit();
            }


        }

        private void AddWindows(Document doc, List<Wall> walls)
        {
            var windowsType = new FilteredElementCollector(doc)
                   .OfClass(typeof(FamilySymbol))
                   .OfCategory(BuiltInCategory.OST_Windows)
                   .OfType<FamilySymbol>()
                   .Where(w => w.Name.Equals("0406 x 1220 мм"))
                   .Where(w => w.FamilyName.Equals("M_Неподвижный"))
                   .FirstOrDefault();

            for (int i = 1; i < walls.Count; i++)
            {               
                var wall = walls[i];
                
                LocationCurve hostCurve = wall.Location as LocationCurve;

                XYZ point1 = hostCurve.Curve.GetEndPoint(0);
                XYZ point2 = hostCurve.Curve.GetEndPoint(1);
                XYZ point = (point1 + point2) / 2;
                XYZ point3 = new XYZ(point.X, point.Y, 3);

                if (!windowsType.IsActive)
                    windowsType.Activate();
                doc.Create.NewFamilyInstance(point3, windowsType, wall, GetLevel1(doc), StructuralType.NonStructural);
                
            }

        }

        private void AddDoors(Document doc, Wall wall)
        {
            var doorType = new FilteredElementCollector(doc)
                   .OfClass(typeof(FamilySymbol))
                   .OfCategory(BuiltInCategory.OST_Doors)
                   .OfType<FamilySymbol>()
                   .Where(d => d.Name.Equals("0915 x 2032 мм"))
                   .Where(d => d.FamilyName.Equals("M_Однопольные-Щитовые"))
                   .FirstOrDefault();

            LocationCurve hostCurve = wall.Location as LocationCurve;

            XYZ point1 = hostCurve.Curve.GetEndPoint(0);
            XYZ point2 = hostCurve.Curve.GetEndPoint(1);
            XYZ point = (point1 + point2) / 2;
            if (!doorType.IsActive)
                doorType.Activate();
            doc.Create.NewFamilyInstance(point, doorType, wall, GetLevel1(doc), StructuralType.NonStructural);
        }

    }
}
