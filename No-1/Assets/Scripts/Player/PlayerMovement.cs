using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : Entity
{
    [Header("Movimiento")]
    public float velocidadMovimiento = 5f;
    [Header("Salto")]
    public float fuerzaSalto = 5f;
    
    [Header("Cámara Fija")]
    public float distanciaCamara = 5f; // Distancia de la cámara al personaje
    public float alturaCamara = 5f;   // Altura de la cámara respecto al personaje
    public float anguloCamara = 45f;  // Ángulo fijo de 45° hacia abajo

    [Header("Debug")]
    public bool mostrarGizmos = true;
    public float longitudGizmo = 2f;
    public Color colorGizmo = Color.blue;
  
    private Rigidbody rb;
    private bool estaEnSuelo;
    private Vector3 movimientoActual;
    private Camera camaraJugador;
    private Transform bodyTransform;
    
    protected override void Awake()
    {
        base.Awake();
        // Buscar el hijo "Body" y obtener su Rigidbody
        bodyTransform = transform.Find("Body");
        if (bodyTransform == null)
        {
            Debug.LogError("No se encontró el objeto hijo 'Body'. Asegúrate de que existe y tiene el nombre correcto.");
            return;
        }
        
        rb = bodyTransform.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("No se encontró el componente Rigidbody en el objeto 'Body'.");
            return;
        }
        
        entityType = EntityType.Player;
        camaraJugador = Camera.main;
        
        // Bloquear y ocultar el cursor (opcional, ya que usaremos el mouse para la rotación)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Configurar la cámara inicialmente
        ActualizarCamara();
    }

    void Update()
    {
        // Detectar si está en el suelo (usando la posición del body)
        estaEnSuelo = Physics.Raycast(bodyTransform.position, Vector3.down, 1.1f);
        
        // Calcular movimiento relativo a la cámara (como en Spiral Knights)
        float movimientoHorizontal = Input.GetAxisRaw("Horizontal");
        float movimientoVertical = Input.GetAxisRaw("Vertical");
        
        // Movimiento relativo a la cámara (ejes globales, no locales al personaje)
        Vector3 direccionCamara = new Vector3(movimientoHorizontal, 0f, movimientoVertical).normalized;
        movimientoActual = direccionCamara * velocidadMovimiento;
        
        // Actualizar estado
        if (movimientoActual.magnitude > 0.1f)
        {
            CurrentState = EntityState.Moving;
        }
        else
        {
            CurrentState = EntityState.Idle;
        }
        
        // Salto
        if (Input.GetKeyDown(KeyCode.Space) && estaEnSuelo)
        {
            rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
        }

        // Hacer que el personaje mire hacia el mouse
        RotarHaciaMouse();

        // Mantener la cámara fija y siguiendo al personaje
        ActualizarCamara();
    }

    void FixedUpdate()
    {
        // Aplicar movimiento en FixedUpdate para físicas
        Move(movimientoActual);

        // Asegurarse de que el Rigidbody no rote en X ni Z
        Vector3 rotacionActual = bodyTransform.rotation.eulerAngles;
        bodyTransform.rotation = Quaternion.Euler(0f, rotacionActual.y, 0f);
    }

    void Move(Vector3 movimiento)
    {
        // Aplicar movimiento directamente (ya es relativo a la cámara)
        Vector3 nuevaVelocidad = new Vector3(movimiento.x, rb.velocity.y, movimiento.z);
        rb.velocity = nuevaVelocidad;
    }

    void ActualizarCamara()
    {
        // Calcular la posición deseada de la cámara (arriba y detrás del personaje, fija)
        Vector3 direccionCamara = new Vector3(0f, alturaCamara, -distanciaCamara); // Detrás y arriba
        Quaternion rotacionCamara = Quaternion.Euler(anguloCamara, 0f, 0f); // Fija en 45° hacia abajo
        Vector3 posicionDeseada = bodyTransform.position + rotacionCamara * direccionCamara;

        // Actualizar la posición y rotación de la cámara
        camaraJugador.transform.position = posicionDeseada;
        camaraJugador.transform.rotation = rotacionCamara;
    }

    void RotarHaciaMouse()
    {
        // Obtener la posición del mouse en pantalla
        Vector3 posicionMouse = Input.mousePosition;
        
        // Convertir la posición del mouse a un rayo en el mundo
        Ray rayoMouse = camaraJugador.ScreenPointToRay(posicionMouse);
        
        // Crear un plano en el suelo (a la altura del personaje) para intersectar con el rayo
        Plane planoSuelo = new Plane(Vector3.up, bodyTransform.position);
        float distancia;
        
        // Si el rayo intersecta el plano, obtener el punto de intersección
        if (planoSuelo.Raycast(rayoMouse, out distancia))
        {
            Vector3 puntoMundo = rayoMouse.GetPoint(distancia);
            
            // Calcular la dirección desde el personaje hacia el punto
            Vector3 direccion = (puntoMundo - bodyTransform.position).normalized;
            direccion.y = 0f; // Ignorar el eje Y para que solo rote en el plano horizontal
            
            // Rotar el personaje hacia esa dirección
            if (direccion != Vector3.zero)
            {
                Quaternion rotacionDeseada = Quaternion.LookRotation(direccion, Vector3.up);
                bodyTransform.rotation = Quaternion.Slerp(bodyTransform.rotation, rotacionDeseada, Time.deltaTime * 10f);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!mostrarGizmos || bodyTransform == null) return;

        // Dibujar una línea que indica la dirección hacia donde mira el personaje
        Gizmos.color = colorGizmo;
        Vector3 posicionInicial = bodyTransform.position;
        Vector3 direccion = bodyTransform.forward * longitudGizmo;
        Gizmos.DrawRay(posicionInicial, direccion);

        // Dibujar una esfera al final de la línea para mejor visibilidad
        Gizmos.DrawSphere(posicionInicial + direccion, 0.1f);
    }

    protected override void Die()
    {
        Debug.Log("Jugador ha muerto");
    }
}