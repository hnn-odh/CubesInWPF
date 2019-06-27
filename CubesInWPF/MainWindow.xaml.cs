using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CubesInWPF
{
    public partial class MainWindow : Window
    {
        HashSet<Cube> items = new HashSet<Cube>();
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {


            InitializeComponent();
            items.Add(new Cube() { Name = "cube1", Color = Colors.BlueViolet });
            items.Add(new Cube() { Name = "cube1", Color = Colors.Red });
            items.Add(new Cube() { Name = "Cube2", Color = Colors.Blue });
            items.Add(new Cube() { Name = "box", Color = Colors.Yellow });
            items.Add(new Cube() { Name = "box2", Color = Colors.Violet });
            listOfCubes.ItemsSource = items;
            CollectionViewSource.GetDefaultView(listOfCubes.ItemsSource).Filter = SearchFilter; //bind filter to the view
        }

        //Filter list based on textual input.
        private bool SearchFilter(object item)
        {
            if (String.IsNullOrEmpty(textBoxSearch.Text))
                return true;
            else
            {
                return ((item as Cube).Name.IndexOf(textBoxSearch.Text, StringComparison.OrdinalIgnoreCase) >= 0);
            }
        }

        private void Textboxsearch_KeyUp(object sender, KeyEventArgs e)
        {

            //add/remove a cube item by pressing enter or delete 
            if (e.Key == Key.Enter)
            {
                //add new item 
                items.Add(new Cube() { Name = textBoxSearch.Text });


            }
            else if (e.Key == Key.Delete)
            {

                try
                {
                    items.Remove(listOfCubes.Items[0] as Cube);
                    textBoxSearch.Text = "";
                }
                catch (Exception)
                {
                    //igonre
                }


            }

            CollectionViewSource.GetDefaultView(listOfCubes.ItemsSource).Refresh();

        }

        private void ListViewItem_Selected(object sender, RoutedEventArgs e)
        {
            Cube sendingItem = ((sender as ListViewItem).Content as Cube);
            if (!viewport3D.Children.Contains(sendingItem.Cube3D))
            {
                viewport3D.Children.Add(sendingItem.Cube3D);//8A2BE2
                (sender as ListViewItem).Background = new SolidColorBrush(Color.FromArgb(0x50, 0x8A, 0xBE, 0xE2));
            }
            else if (e.RoutedEvent.Name == "MouseDoubleClick")
            {
                viewport3D.Children.Remove(sendingItem.Cube3D);
                (sender as ListViewItem).Background = new SolidColorBrush(Colors.Transparent);
            }
            new Task(() =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    CollectionViewSource.GetDefaultView(listOfCubes.ItemsSource).Refresh();

                }));
            });

        }



        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && Key.Add == e.Key)
            {

                Zoom(-1);
            }
            else if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && Key.Subtract == e.Key)
            {

                Zoom(1);
            }
            else if (Key.Escape == e.Key)
            {
                this.Close();

            }
        }

        void Zoom(int num)
        {
            viewportCamera.Position = new Point3D(viewportCamera.Position.X + num, viewportCamera.Position.Y + num, viewportCamera.Position.Z + num);
        }
    }


}



class Cube
{
    public string Name { get; set; }
    public double Height { get; private set; } = 5; //defualt value
    public double Depth { get; private set; } = 5;
    public double Width { get; private set; } = 5;
    public ModelVisual3D Cube3D { get; private set; } = new ModelVisual3D();
    public Transform3DGroup Transform
    {
        get
        {
            return Cube3D.Transform as Transform3DGroup;
        }

        set
        {
            Cube3D.Transform = value;
        }
    }
    public ScaleTransform3D ScaleTransform
    {
        set
        {
            if (Transform.Children.Count > 0)
            {
                Transform.Children[0] = value;
            }
            else
            {
                Transform.Children.Add(value);
            }

        }

        get
        {
            return Transform.Children[0] as ScaleTransform3D;
        }
    }
    public RotateTransform3D RotateTransform
    {
        set
        {
            if (Transform.Children.Count > 1)
            {
                Transform.Children[1] = value;
            }
            else
            {
                Transform.Children.Add(value);
            }
        }
        get
        {
            return Transform.Children[1] as RotateTransform3D;
        }
    }
    public TranslateTransform3D TranslateTransform
    {

        set
        {
            if (Transform.Children.Count > 2)
            {
                Transform.Children[2] = value;
            }
            else
            {
                Transform.Children.Add(value);
            }

        }
        get
        {
            return Transform.Children[2] as TranslateTransform3D;
        }
    }
    public Color Color
    {
        get
        {
            return ((((Cube3D.Content as Model3DGroup).Children[0] as GeometryModel3D)
                .Material as DiffuseMaterial).Brush as SolidColorBrush).Color;
        }
        set
        {
            var length = (Cube3D.Content as Model3DGroup).Children.Count;
            float f = 0;
            for (int i = 0; i < 6; i++)
            {
                f = 100f - (float)(i * 12f);
                ((Cube3D.Content as Model3DGroup).Children[i] as GeometryModel3D)
                    .Material = new DiffuseMaterial(new SolidColorBrush(DarkerColor(value, f)));
            }
        }
    }
    public Cube()
    {
        MakeCube();
    }
    private Color DarkerColor(Color color, float correctionfactory = 50f)
    {
        const float hundredpercent = 100f;
        return Color.FromArgb(color.A, (byte)(((float)color.R / hundredpercent) * correctionfactory),
            (byte)(((float)color.G / hundredpercent) * correctionfactory), 
            (byte)(((float)color.B / hundredpercent) * correctionfactory));
    }
    private ModelVisual3D MakeCube()
    {
        Point3D p0, p1, p2, p3, p4, p5, p6, p7;
        int[] surfaceConnections = { 2, 7, 6, 3, 7, 2,
            0, 5, 4, 1, 5, 0, 2, 4, 6, 0, 4, 2, 5, 3, 1,
            7, 3, 5, 0, 3, 1, 2, 3, 0, 7, 5, 6, 6, 5, 4 };
        MeshGeometry3D[] meshGeometry3D = new MeshGeometry3D[6];
        Point3DCollection pointCollection = new Point3DCollection();
        GeometryModel3D[] geometryModel = new GeometryModel3D[6];
        Model3DGroup model3DGroup = new Model3DGroup();
        Int32Collection triangeles;

        //get cube vertex
        p0 = new Point3D(0, 0, 0);
        p1 = new Point3D(0, 0, Depth);
        p2 = new Point3D(0, Height, 0);
        p3 = new Point3D(0, Height, Depth);
        p4 = new Point3D(Width, 0, 0);
        p5 = new Point3D(Width, 0, Depth);
        p6 = new Point3D(Width, Height, 0);
        p7 = new Point3D(Width, Height, Depth);

        pointCollection.Add(p0);
        pointCollection.Add(p1);
        pointCollection.Add(p2);
        pointCollection.Add(p3);
        pointCollection.Add(p4);
        pointCollection.Add(p5);
        pointCollection.Add(p6);
        pointCollection.Add(p7);
        //Each cube side has it's own geometry
        for (int i = 0; i < 6; i++)
        {
            meshGeometry3D[i] = new MeshGeometry3D();
            meshGeometry3D[i].Positions = pointCollection;
            geometryModel[i] = new GeometryModel3D();
            triangeles = new Int32Collection();
            for (int j = 0; j < 6; j++)
            {
                triangeles.Add(surfaceConnections[i * 6 + j]);
            }
            meshGeometry3D[i].TriangleIndices = triangeles;
            geometryModel[i].Geometry = meshGeometry3D[i];
            model3DGroup.Children.Add(geometryModel[i]);
        }
        Cube3D.Content = model3DGroup;
        Transform = new Transform3DGroup();
        this.ScaleTransform = new ScaleTransform3D(1, 1, 1);
        this.RotateTransform = new RotateTransform3D();
        RotateTransform.Rotation = new AxisAngleRotation3D()
        {
            Axis = new Vector3D(1, 1, 1),
            Angle = 0
        };
        this.TranslateTransform = new TranslateTransform3D(0, 0, 0);
        Cube3D.Transform = Transform;
        return Cube3D;
    }
    //override GetHashCode() & Equals (object) for a Unique HashSet    
    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
    public override bool Equals(object other)
    {
        if (other as Cube is null) return false;
        return this.Name.Equals((other as Cube).Name);
    }
}

