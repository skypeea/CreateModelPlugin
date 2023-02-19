using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateModelPlugin
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreateModelPlugin : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            double width = UnitUtils.ConvertToInternalUnits(10000, UnitTypeId.Millimeters);
            double depth = UnitUtils.ConvertToInternalUnits(5000, UnitTypeId.Millimeters);

            List<Level> levels = GetLevelList(doc);
            Level level1 = GetLevelByName(levels, "Уровень 1");
            Level level2 = GetLevelByName(levels, "Уровень 2");
            List<XYZ> points = GetPointsByWidthAndDepth(width, depth);

            List<Wall> wallList = CreateWall(doc, points, level1, level2);


            return Result.Succeeded;
        }
        public List<Level> GetLevelList(Document doc)
        {
            List<Level> listLevel = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .OfType<Level>()
                .ToList();
            return listLevel;
        }
        public Level GetLevelByName(List<Level> levelList, string levelName)
        {
            return levelList
                .Where(x => x.Name.Equals(levelName))
                .FirstOrDefault();

        }
        public List<Wall> CreateWall(Document doc, List<XYZ> points, Level bottomLevel, Level topLevel)
        {

            using (var ts = new Transaction(doc, "Create walls"))
            {
                List<Wall> walls = new List<Wall>();
                ts.Start();
                for (int i = 0; i < 4; i++)
                {
                    Line line = Line.CreateBound(points[i], points[i + 1]);
                    Wall wall = Wall.Create(doc, line, bottomLevel.Id, false);
                    walls.Add(wall);
                    wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(topLevel.Id);

                }
                return walls;
                ts.Commit();
            }
        }
        public List<XYZ> GetPointsByWidthAndDepth(double width, double depth)
        {
            double dx = width / 2;
            double dy = depth / 2;

            List<XYZ> points = new List<XYZ>();
            points.Add(new XYZ(-dx, -dy, 0));
            points.Add(new XYZ(dx, -dy, 0));
            points.Add(new XYZ(dx, dy, 0));
            points.Add(new XYZ(-dx, dy, 0));
            points.Add(new XYZ(-dx, -dy, 0));
            return points;
        }
    }
}
