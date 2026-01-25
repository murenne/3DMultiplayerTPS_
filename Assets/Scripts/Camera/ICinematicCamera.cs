using Quantum;
using Quantum.TPSroject;

public interface ICinematicCamera
{
    CinematicCameraType cinematicCameraType { get; }
    void Play();
    void End();
}
