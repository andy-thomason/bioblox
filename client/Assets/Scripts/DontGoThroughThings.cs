using UnityEngine;
using System.Collections;

public class DontGoThroughThings : MonoBehaviour
{
    // Careful when setting this to true - it might cause double
    // events to be fired - but it won't pass through the trigger
    public bool sendTriggerMessage = false;

    public LayerMask layerMask = -1; //make sure we aren't in this layer 
    public float skinWidth = 0.1f; //probably doesn't need to be changed 

    private float minimumExtent;
    private float partialExtent;
    private float sqrMinimumExtent;
    private Vector3 previousPosition;
    private Rigidbody myRigidbody;
    private Collider myCollider;

    BioBlox bb;

    //initialize values 
    void Start()
    {
        myRigidbody = GetComponent<Rigidbody>();
        myCollider = GetComponent<Collider>();
        previousPosition = myRigidbody.position;
        //minimumExtent = Mathf.Min(Mathf.Min(myCollider.bounds.extents.x, myCollider.bounds.extents.y), myCollider.bounds.extents.z);
        minimumExtent = 1.0f;
        partialExtent = minimumExtent * (1.0f - skinWidth);
        sqrMinimumExtent = minimumExtent * minimumExtent;
        bb = FindObjectOfType<BioBlox>();
    }

    void FixedUpdate()
    {
        //have we moved more than our minimum extent? 
        Vector3 movementThisStep = myRigidbody.position - previousPosition;
        float movementSqrMagnitude = movementThisStep.sqrMagnitude;
        
            float movementMagnitude = Mathf.Sqrt(movementSqrMagnitude);

            BvhSphereCollider space_ship = new BvhSphereCollider(bb.molecules[0].GetComponent<PDB_mesh>().mol, bb.molecules[0].transform, transform.position, 2.0f);

            if (space_ship.results.Count != 0)
            {

                Debug.Log("si");
                myRigidbody.position = bb.molecules[0].transform.TransformPoint(bb.molecules[0].GetComponent<PDB_mesh>().mol.atom_centres[space_ship.results[0].index]) - (movementThisStep / movementMagnitude) * partialExtent;

            }

        previousPosition = myRigidbody.position;
    }
}