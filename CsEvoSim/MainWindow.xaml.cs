using CsEvoSim.Components;
using CsEvoSim.Core;
using CsEvoSim.Systems;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CsEvoSim;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private World _world;
    private DispatcherTimer _timer;

    public MainWindow()
    {
        InitializeComponent();

        _world = new World();
        _world.AddSystem(new MovementSystem());
        _world.AddSystem(new RenderSystem(SimulationCanvas));

        // Spawn initial entity
        var entity = new Entity();
        entity.AddComponent(new PositionComponent(100, 100));
        entity.AddComponent(DNAComponent.Random());
        _world.AddEntity(entity);

        // Start the simulation loop
        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
        _timer.Tick += (_, _) => _world.Update();
        _timer.Start();

    }
}