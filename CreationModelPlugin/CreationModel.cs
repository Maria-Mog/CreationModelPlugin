using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
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
                AddRoof(doc, walls);
                ts.Commit();
            }

            //ROOF_CONSTRAINT_OFFSET_PARAM -смещение уровня

        }

        private void AddRoof(Document doc, List<Wall> walls)
        {
            RoofType roofType = new FilteredElementCollector(doc)
                .OfClass(typeof(RoofType))
                .OfType<RoofType>()
                .Where(r => r.Name.Equals("Типовая крыша - 500мм"))
                .Where(r => r.FamilyName.Equals("Базовая крыша"))
                .FirstOrDefault();

            double roofWidth = walls[0].Width;
            double rt = roofWidth;
            
            List<XYZ> points = new List<XYZ>();
            points.Add(new XYZ(-rt, -rt, 0));
            points.Add(new XYZ(rt, -rt, 0));
            points.Add(new XYZ(rt, rt, 0));
            points.Add(new XYZ(-rt, rt, 0));
            points.Add(new XYZ(-rt, -rt, 0));

            CurveArray curveArray = new CurveArray();
            LocationCurve curve = walls[0].Location as LocationCurve;
            XYZ p1 = curve.Curve.GetEndPoint(0) + points[0];
            XYZ p2 = curve.Curve.GetEndPoint(1) + points[1];
            XYZ p3 = (p1 + p2) / 2;
            LocationCurve curve2 = walls[1].Location as LocationCurve;
            XYZ p4 = curve2.Curve.GetEndPoint(0) + points[0];
            XYZ p5 = curve2.Curve.GetEndPoint(1) + points[3];
            XYZ p6 = (p4 + p5) / 2;
            LocationCurve curve3 = walls[3].Location as LocationCurve;
            XYZ p7 = curve3.Curve.GetEndPoint(0) + points[1];
            XYZ p8 = curve3.Curve.GetEndPoint(1) + points[2];
            XYZ p9 = (p7 + p8) / 2;

            curveArray.Append(Line.CreateBound(new XYZ(p1.X, p1.Y, 13.12), new XYZ(p3.X, p3.Y, 23.12)));
            curveArray.Append(Line.CreateBound(new XYZ(p3.X, p3.Y, 23.12), new XYZ(p2.X, p2.Y, 13.12)));
            ReferencePlane referencePlane = doc.Create.NewReferencePlane(new XYZ(p6.X, p6.Y, 13.12),
                                                                         new XYZ(p9.X, p9.Y, 13.12),
                                                                         new XYZ(0, 0, 10),
                                                                         doc.ActiveView);

            ExtrusionRoof extrusionRoof = doc.Create.NewExtrusionRoof(curveArray, referencePlane, GetLevel2(doc), roofType, p4.Y, p5.Y);
        }

        //private void AddRoof(Document doc, List<Wall> walls)
        //{
        //    RoofType roofType = new FilteredElementCollector(doc)
        //        .OfClass(typeof(RoofType))
        //        .OfType<RoofType>()
        //        .Where(r => r.Name.Equals("Типовая крыша - 500мм"))
        //        .Where(r => r.FamilyName.Equals("Базовая крыша"))
        //        .FirstOrDefault();

        //    double roofWidth = walls[0].Width;
        //    double rt = roofWidth / 2;

        //    List<XYZ> points = new List<XYZ>();
        //    points.Add(new XYZ(-rt, -rt, 0));
        //    points.Add(new XYZ(rt, -rt, 0));
        //    points.Add(new XYZ(rt, rt, 0));
        //    points.Add(new XYZ(-rt, rt, 0));
        //    points.Add(new XYZ(-rt, -rt, 0));


        //    Application application = doc.Application;
        //    CurveArray footPrint = application.Create.NewCurveArray();
        //    for (int i = 0; i < 4; i++)
        //    {
        //        LocationCurve curve = walls[i].Location as LocationCurve;
        //        XYZ p1 = curve.Curve.GetEndPoint(0);
        //        XYZ p2 = curve.Curve.GetEndPoint(1);
        //        Line line = Line.CreateBound(p1 + points[i], p2 + points[i + 1]);
        //        footPrint.Append(line);
        //    }
        //    ModelCurveArray modelCurve = new ModelCurveArray();
        //    FootPrintRoof footPrintRoof = doc.Create.NewFootPrintRoof(footPrint, GetLevel2(doc), roofType, out modelCurve);
        //    foreach (ModelCurve m in modelCurve)
        //    {
        //        footPrintRoof.set_DefinesSlope(m, true);
        //        footPrintRoof.set_SlopeAngle(m, 0.5);
        //    }
        //}

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
