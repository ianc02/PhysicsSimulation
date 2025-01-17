using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class Gravity : MonoBehaviour
{
    internal Rigidbody2D rbody;
    public Vector2 startVelocity = Vector2.zero;
    TrailRenderer trail;
    internal bool beingDragged;
    public GameObject explosion;
    float screenSize;
    internal bool selected = false;
    private System.DateTime now;
    private float timeValue;

    // Start is called before the first frame update
    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
        trail = GetComponent<TrailRenderer>();
        rbody.velocity = startVelocity;
        SetSize();
    }

    private void FixedUpdate()
    {
        if (!beingDragged)
        {
            Vector2 v = GameManager.Instance.deltaV(this);
            if (!IsInvalid(v))
                rbody.AddForce(v);
        }
    }

    private void OnMouseDown()
    {
		now = System.DateTime.Now.AddSeconds(.1);
    }

    private void OnMouseDrag()
    {
        if (System.DateTime.Now > now)
        {
            beingDragged = true;
            startVelocity = rbody.velocity;
        }
        if (beingDragged)
        {
            rbody.velocity = Vector2.zero;
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pos.z = 0;
            this.gameObject.transform.position = pos;
            timeValue = GameManager.Instance.getTimeValue();
            GameManager.Instance.pauseTime();
        }
    }

    private void OnMouseUp()
    {

        if (beingDragged)
        {
            beingDragged = false;
            rbody.velocity = startVelocity;
            if (!GameManager.Instance.autoPause.GetComponent<Toggle>().isOn)
            {
                GameManager.Instance.setTimeScale(timeValue);
            }
        }
        else 
        {
            if (GameManager.Instance.select1 && GameManager.Instance.select2 && !GameManager.Instance.s1.Equals(this) && !GameManager.Instance.s2.Equals(this))
            {
                GameManager.Instance.s1.ChangeMaterial();
            }
            ChangeMaterial();
        }
    }

    public void ChangeMaterial()
    {
        if (!selected)
        {
            rbody.GetComponentInParent<SpriteRenderer>().material = GameManager.Instance.notSelect;
            if (!GameManager.Instance.select1)
            {
                GameManager.Instance.s1 = this;
                GameManager.Instance.select1 = true;
                GameManager.Instance.highlight(true, true, false);
            }
            else if (!GameManager.Instance.select2)
			{
                GameManager.Instance.s2 = this;
                GameManager.Instance.select2 = true;
                GameManager.Instance.highlight(true, false, true);
            }
            else
			{
                GameManager.Instance.s1 = this;
                GameManager.Instance.select1 = true;
                GameManager.Instance.highlight(true, true, false);
            }
            selected = true;
        }
        else
        {
            rbody.GetComponentInParent<SpriteRenderer>().material = GameManager.Instance.select;
            if (!(GameManager.Instance.s2 is null) && GameManager.Instance.s2.Equals(this))
            {
                GameManager.Instance.highlight(false, false, true);
                GameManager.Instance.s2 = null;
                GameManager.Instance.select2 = false;
                

            }
            else
            {
                GameManager.Instance.highlight(false, true, false);
                GameManager.Instance.s1 = null;
                GameManager.Instance.select1 = false;
                
            }
            selected = false;
        }
	}

    public string getTag()
    {
        return rbody.gameObject.transform.parent.gameObject.tag;
    }

    bool IsInvalid(Vector2 v) {
        return float.IsNaN(v.x) || float.IsInfinity(v.x)
            || float.IsNaN(v.y) || float.IsInfinity(v.y);
    }

    private void OnDrawGizmos()
    {
        if (rbody)
        {
            Gizmos.DrawRay(rbody.position, rbody.velocity);
        }
    }


    public void changeMass(float m)
    {
        rbody.mass = m;
        SetSize();
    }

    public void changePosX(float m)
    {
        rbody.transform.position = new Vector3(m, rbody.position.y, 0);
    }

    public void changePosY(float m)
    {
        rbody.position = new Vector3(rbody.position.x, m, 0);
    }

    public void changeVelocityX(float m)
    {
        rbody.velocity = new Vector3(m, rbody.velocity.y, 0);
    }

    public void changeVelocityY(float m)
    {
        rbody.velocity = new Vector3(rbody.velocity.x, m, 0);
    }


    public void changeTrailLength(float l)
    {
        trail.time = l;
    }

    public void changeColor(Color i)
    {
        rbody.GetComponentInParent<SpriteRenderer>().color = i;
    }

    void SetSize()
    {
        screenSize = 1+Mathf.Log(rbody.mass)/6;
        this.gameObject.transform.localScale = Vector3.one * screenSize;
    }

    public void changeVelocity(Vector2 v)
    {
        this.rbody.velocity = v;
    }

    public void delete()
    {
        Destroy(rbody.gameObject.transform.parent.gameObject);
    }

    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("trigger collision");
        Gravity otherGrav = collision.gameObject.GetComponent<Gravity>();

        //The collided object that survives is either the more massive one, or arbitrarily decided

        if (this.rbody.mass > otherGrav.rbody.mass ||
            (this.rbody.mass == otherGrav.rbody.mass && Compare(otherGrav)))
        {
            Vector2 netMomentum = rbody.velocity * rbody.mass + otherGrav.rbody.velocity * otherGrav.rbody.mass;
            this.rbody.mass += otherGrav.rbody.mass;
            this.rbody.velocity = netMomentum / this.rbody.mass;
            SetSize();
        } else {
            GameManager.Instance.DestroyBody(this);
            GameObject e = Instantiate(explosion) as GameObject;
            e.transform.position = transform.position;
            e.GetComponent<Animator>().runtimeAnimatorController.animationClips[0].wrapMode = WrapMode.Once;
        }

    }

    bool Compare(Gravity other){

        return this.gameObject.transform.position.x > other.gameObject.transform.position.x
            ;

    }


}