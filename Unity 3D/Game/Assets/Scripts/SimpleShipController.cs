using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleShipController : MonoBehaviour
{

    private Vector2 delta = Vector2.zero;
    public float speed = 5.0f;
    public float padding = 1f;
    public GameObject beam;
    public GameObject turret;
    private GameObject instantiatedbeam;
    private bool firing;

    [SerializeField]
    private Vector3 rightPos;
    [SerializeField]
    private Vector3 leftPos;
    [SerializeField]
    private Vector3 originalPos;

    public Vector3 dirCheck;

    Vector3 startPosition;
    Vector3 endPosition;

    public Transform hitTransform;

    void Start()
    {
        firing = false;
        originalPos = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 2.5f, gameObject.transform.position.z);
        rightPos = new Vector3(gameObject.transform.position.x + 0.5f, gameObject.transform.position.y + 2.5f, gameObject.transform.position.z);
        leftPos = new Vector3(gameObject.transform.position.x - 0.5f, gameObject.transform.position.y + 2.5f, gameObject.transform.position.z);
    }

    void Fire()
    {
        Destroy(instantiatedbeam);
        instantiatedbeam = Instantiate(beam, transform.position, Quaternion.identity) as GameObject;
    }

    void ScaleAndRotateBeam(Transform From, Transform To)
    {
        if (instantiatedbeam != null)
        {
            var beam = instantiatedbeam.transform;
            Vector3 startPos = turret.transform.position;
            Vector3 endPos = To.position;

            var dir = endPos - startPos;
            var mid = (dir) / 2.0f + startPos;

            dirCheck = dir;
            beam.position = mid;
            beam.position = new Vector3(beam.position.x, beam.position.y, beam.position.z + 0.01f);
            beam.rotation = Quaternion.FromToRotation(Vector3.up, dir);
            beam.Rotate(Vector3.forward * -180);
            Vector3 scale = beam.localScale;
            scale.y = dir.magnitude * 0.5f;
            beam.localScale = scale;
        }
    }

    // Update is called once per frame
    void Update()
    {
        originalPos = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 2.2f, gameObject.transform.position.z);

        rightPos = new Vector3(gameObject.transform.position.x + 0.5f, gameObject.transform.position.y + 2.2f, gameObject.transform.position.z);

        leftPos = new Vector3(gameObject.transform.position.x - 0.5f, gameObject.transform.position.y + 2.2f, gameObject.transform.position.z);

        turret.transform.position = originalPos;

        if (dirCheck.x > 2.0f)
        {
            turret.transform.position = rightPos;
        }
        else if (dirCheck.x < -2.0f)
        {
            turret.transform.position = leftPos;
        }
        else
        {
            turret.transform.position = originalPos;
        }

        if (Input.GetMouseButton(0) && !firing)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                firing = true;
                Invoke("Fire", 0.1f);
                ScaleAndRotateBeam(transform, hit.transform);
                Debug.Log("Hit " + hit.transform.name + "!");
                hitTransform = hit.transform;
            }
        }
        if (!Input.GetMouseButton(0) && firing)
        {
            firing = false;
            Destroy(instantiatedbeam);
            hitTransform = null;
        }
        if (Input.GetMouseButton(0) && firing)
        {
            if (hitTransform)
            {
                ScaleAndRotateBeam(transform, hitTransform);
                Debug.Log("Dragging " + hitTransform.name + "!");
            }
        }

        transform.Translate(0, delta.y, 0);
        transform.Translate(delta.x, 0, 0);
    }
}
