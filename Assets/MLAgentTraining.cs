using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class MLAgentTraining : Agent
{
    [SerializeField] private Transform goalPosition;
    [SerializeField] private bool tope;
    RaycastHit hitInfo = new();
    float objeto = 0;
    [SerializeField] float distancia;
    int puntuacion = 0;

    List<Vector3> posicionesGoals = new List<Vector3>() { new Vector3(8, 0, 8), new Vector3(-8, 0, 8), new Vector3(-8, 0, -8), new Vector3(8, 0, -8) };

    [SerializeField] MeshRenderer floor;
    [SerializeField] List<GameObject> obstacles;
    [SerializeField] GameObject environment;
    public override void OnEpisodeBegin()
    {
        int[] rot = { 0, 90, 180, -90 };
        environment.transform.eulerAngles = new Vector3(0, rot[Random.Range(0, 4)], 0);

        int v = Random.Range(0, posicionesGoals.Count);
        int r = Random.Range(0, obstacles.Count);

        foreach (GameObject g in obstacles) 
        {
            g.SetActive(false);
        }
        obstacles[r].SetActive(true);

        //transform.localPosition = new Vector3(Random.Range(-9, 9), 0, Random.Range(-9, 9));
        transform.localPosition = new Vector3(-5, 0, 0);
        goalPosition.localPosition = new Vector3(8, 0, 0);
        //goalPosition.localPosition = posicionesGoals[v];
        puntuacion = 0;
        //floor.material.color= Color.white;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position);
        sensor.AddObservation(goalPosition.position);
        sensor.AddObservation(objeto);
        sensor.AddObservation(distancia);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        //Movimiento
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        Vector3 Direccion = new Vector3(moveX, 0, moveZ);
        Direccion.Normalize();

        float moveSpeed = 1f;
        transform.position += moveSpeed * Time.deltaTime * new Vector3(moveX, 0, moveZ);

        //Vision (Raycast)
        /*if (Physics.Raycast(transform.position, Direccion, out hitInfo, 8))
        {
            if (hitInfo.collider.CompareTag("Obstacle"))
            {
                objeto = 2;
                SetReward(-0.1f);
            }
            else if (hitInfo.collider.CompareTag("Goal"))
            {
                objeto = 1;
                SetReward(1);
            }
            else
            {
                objeto = 0;
            }
        }*/
        Debug.DrawRay(transform.position, transform.forward * 8, Color.red);

        if (Direccion != Vector3.zero) 
        {
            transform.forward = Direccion;
        }

        //Distancia
        distancia = Vector3.Distance(transform.position, goalPosition.position);

        //if (distancia < 2) SetReward(-1);
        if (distancia < 4 && distancia > 2) 
        {
            //puntuacion += 1;
            SetReward(1);
            floor.material.color = Color.green;
            EndEpisode();
        }
        /*else SetReward(-1);

        if (puntuacion >= 500) 
        {
            SetReward(10);
            EndEpisode();
        }*/
        //Debug.Log(puntuacion);
        //Debug.Log(objeto);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuosActions = actionsOut.ContinuousActions;

        continuosActions[0] = Input.GetAxisRaw("Horizontal");
        continuosActions[1] = Input.GetAxisRaw("Vertical");
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal")) 
        {
            SetReward(-5);
            EndEpisode();
        }

        if (other.CompareTag("Wall"))
        {
            SetReward(-1);
            floor.material.color = Color.red;
            EndEpisode();
        }

        if (other.CompareTag("Obstacle")) 
        {
            Debug.Log("Contact");
            SetReward(-1f);
            floor.material.color = Color.red;
            EndEpisode();
        }
    }
}
