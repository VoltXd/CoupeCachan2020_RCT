using System;
using System.Collections.Generic;
using System.Drawing;
using SharpGL;

namespace GeometryElements
{
    public static class OpenGLDisplayerExtensions
    {
        public static void Rotate(this OpenGL gl, double anglex, double angley, double anglez)
        {
            gl.Rotate(anglex, 1, 0, 0);
            gl.Rotate(angley, 0, 1, 0);
            gl.Rotate(anglez, 0, 0, 1);
        }

        public static void Draw(this Element element, OpenGL gl)
        {
            switch (element)
            {
                case Polygon polygon:
                    polygon.Draw(gl);
                    break;
                case OutlinedCircle outlinedCircle:
                    outlinedCircle.Draw(gl);
                    break;
                case Circle circle:
                    circle.Draw(gl);
                    break;
                case Segment segment:
                    segment.Draw(gl);
                    break;
                case Triangle triangle:
                    triangle.Draw(gl);
                    break;
                case PointList pointList:
                    pointList.Draw(gl);
                    break;
                case ElementList elementList:
                    elementList.Draw(gl);
                    break;
                default:
                    // Élément sans affichage OpenGL
                    break;
            }
        }

        public static void Draw(this ElementList elementList, OpenGL gl)
        {
            foreach (Element element in elementList.Elements)
                element.Draw(gl);
        }

        public static void Draw(this Point3D p, OpenGL gl, double radius, Color color)
        {
            new Circle(radius, p, color, "point").Draw(gl, 45);
        }

        public static void Draw(this Polygon polygon, OpenGL gl)
        {
            gl.Begin(polygon.IsFilled ? OpenGL.GL_POLYGON : OpenGL.GL_LINE_LOOP);
            gl.Color(polygon.Color.R, polygon.Color.G, polygon.Color.B, polygon.Color.A);

            foreach (Point3D pt in polygon.Points)
                gl.Vertex(pt.X, pt.Y, pt.Z);
            gl.End();
        }

        public static void Draw(this Circle circle, OpenGL gl, double incrementAngle = 15)
        {
            Point3D center = circle.Position;
            double radius = circle.Radius;

            gl.PushMatrix();

            // Gère la rotation de l'objet
            gl.Translate(circle.Position.X, circle.Position.Y, circle.Position.Z);
            gl.Rotate(circle.Orientation.X, circle.Orientation.Y, circle.Orientation.Z);
            gl.Translate(-circle.Position.X, -circle.Position.Y, -circle.Position.Z);

            gl.Begin(OpenGL.GL_TRIANGLE_FAN);
            gl.Color(circle.Color.R, circle.Color.G, circle.Color.B, circle.Color.A);

            // Dessine le point central duquel partiront les triangles
            gl.Vertex(center.X, center.Y, center.Z);

            for (double theta = circle.AngleStart; theta < circle.AngleStop; theta += incrementAngle)
                gl.Vertex(radius * Math.Cos(Math.PI / 180 * theta) + center.X, radius * Math.Sin(Math.PI / 180 * theta) + center.Y, center.Z);

            // Dessine le dernier point pour terminer le cercle
            gl.Vertex(radius * Math.Cos(Math.PI / 180 * circle.AngleStop) + center.X, radius * Math.Sin(Math.PI / 180 * circle.AngleStop) + center.Y, center.Z);

            gl.End();
            gl.PopMatrix();
        }

        public static void Draw(this OutlinedCircle outlinedCircle, OpenGL gl)
        {
            Point3D center = outlinedCircle.Position;
            double r = outlinedCircle.Radius;
            double t = outlinedCircle.Thickness;

            gl.PushMatrix();

            // Gère la rotation de l'objet
            gl.Translate(outlinedCircle.Position.X, outlinedCircle.Position.Y, outlinedCircle.Position.Z);
            gl.Rotate(outlinedCircle.Orientation.X, outlinedCircle.Orientation.Y, outlinedCircle.Orientation.Z);
            gl.Translate(-outlinedCircle.Position.X, -outlinedCircle.Position.Y, -outlinedCircle.Position.Z);

            // Dessine des points en enchaînant intérieur puis extérieur du périmètre du cercle (triangles reliés grâce à TRIANGLE_STRIP)
            gl.Begin(OpenGL.GL_TRIANGLE_STRIP);
            gl.Color(outlinedCircle.Color.R, outlinedCircle.Color.G, outlinedCircle.Color.B, outlinedCircle.Color.A);

            // Dessine le premier point pour commencer le cercle
            gl.Vertex((r - t) * Math.Cos(Math.PI / 180 * outlinedCircle.AngleStart) + center.X, (r - t) * Math.Sin(Math.PI / 180 * outlinedCircle.AngleStart) + center.Y, center.Z);
            gl.Vertex(r * Math.Cos(Math.PI / 180 * outlinedCircle.AngleStart) + center.X, r * Math.Sin(Math.PI / 180 * outlinedCircle.AngleStart) + center.Y, center.Z);

            // Dessine le point à l'intérieur puis celui à l'extérieur
            double incrementAngle = 15;
            for (double theta = outlinedCircle.AngleStart + incrementAngle; theta < outlinedCircle.AngleStop; theta += incrementAngle)
            {
                gl.Vertex((r - t) * Math.Cos(Math.PI / 180 * (theta - incrementAngle / 2)) + center.X, (r - t) * Math.Sin(Math.PI / 180 * (theta - incrementAngle / 2)) + center.Y, center.Z);
                gl.Vertex(r * Math.Cos(Math.PI / 180 * theta) + center.X, r * Math.Sin(Math.PI / 180 * theta) + center.Y, center.Z);
            }

            // Dessine le dernier point pour terminer le cercle
            gl.Vertex((r - t) * Math.Cos(Math.PI / 180 * outlinedCircle.AngleStop) + center.X, (r - t) * Math.Sin(Math.PI / 180 * outlinedCircle.AngleStop) + center.Y, center.Z);
            gl.Vertex(r * Math.Cos(Math.PI / 180 * outlinedCircle.AngleStop) + center.X, r * Math.Sin(Math.PI / 180 * outlinedCircle.AngleStop) + center.Y, center.Z);

            gl.End();
            gl.PopMatrix();
        }

        public static void Draw(this Segment segment, OpenGL gl)
        {
            gl.Begin(OpenGL.GL_LINES);
            gl.Color(segment.Color.R, segment.Color.G, segment.Color.B, segment.Color.A);

            foreach (Point3D pt in segment.Points)
                gl.Vertex(pt.X, pt.Y, pt.Z);
            gl.End();
        }

        public static void Draw(this Triangle triangle, OpenGL gl)
        {
            gl.Begin(OpenGL.GL_TRIANGLES);
            gl.Color(triangle.Color.R, triangle.Color.G, triangle.Color.B, triangle.Color.A);

            foreach (Point3D pt in triangle.Points)
                gl.Vertex(pt.X, pt.Y, pt.Z);
            gl.End();
        }

        public static void Draw(this PointList pointList, OpenGL gl)
        {
            switch (pointList.Type)
            {
                case PointList.PointListType.NotLinked:
                    gl.Begin(OpenGL.GL_POINTS);
                    break;
                case PointList.PointListType.Linked:
                    gl.Begin(OpenGL.GL_LINE_STRIP);
                    break;
                case PointList.PointListType.Loop:
                    gl.Begin(OpenGL.GL_LINE_LOOP);
                    break;
                default:
                    break;
            }

            gl.Color(pointList.Color.R, pointList.Color.G, pointList.Color.B, pointList.Color.A);

            foreach (Point3D pt in pointList.Points)
                gl.Vertex(pt.X, pt.Y, pt.Z);

            gl.End();
        }
    }
}
