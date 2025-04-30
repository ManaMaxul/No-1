/// <summary>
/// Interfaz que define la estructura base para los estados de la máquina de estados finitos.
/// Define los métodos que todo estado debe implementar para su ciclo de vida.
/// </summary>
public interface IState
{
    /// <summary>
    /// Se ejecuta cuando se entra en el estado.
    /// </summary>
    void OnEnter();

    /// <summary>
    /// Se ejecuta en cada frame mientras el estado está activo.
    /// </summary>
    void OnUpdate();

    /// <summary>
    /// Se ejecuta cuando se sale del estado.
    /// </summary>
    void OnExit();
}