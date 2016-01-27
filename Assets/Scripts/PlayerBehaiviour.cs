﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerBehaiviour : MonoBehaviour, ITR {
    public MoveSettings moveSettings;
    public InputSettings inputSettings;
    public Transform spawnPoint;
    public GameObject Owl, Lemur;
    private Vector2 owlVelocity, lemurVelocity;
    private float p1SidewaysInput, p2SidewaysInput, p1JumpInput, p2JumpInput;
    public LayerMask Layers;

	private TimeReverse trscript;

	public static Text playerStats; //speichert Text

	public bool timereverse;

    private void Awake()
    {
        //Owl = Camera.current.GetComponent<cameraScript>().Owl;
        //Lemur = Camera.current.GetComponent<cameraScript>().Lemur;
        owlVelocity = Vector3.zero;
        p1SidewaysInput = p1JumpInput = 0;
        lemurVelocity = Vector3.zero;
        p2SidewaysInput = p2JumpInput = 0;
		Gamedata.Instance.Lives = 5; //Leben festlegen

    }

    void GetPlayer1Input()
    {
        p1SidewaysInput = Input.GetAxis(inputSettings.PLAYER1_SIDEWAYS_AXIS);
        p1JumpInput = Input.GetAxisRaw(inputSettings.PLAYER1_JUMP_AXIS);
    }

    void GetPlayer2Input()
    {
        p2SidewaysInput = Input.GetAxis(inputSettings.PLAYER2_SIDEWAYS_AXIS);
        p2JumpInput = Input.GetAxisRaw(inputSettings.PLAYER2_JUMP_AXIS);
    }

    void Run()
    {
        if (Owl.transform.position.x < Camera.current.transform.position.x - 8)
            Owl.transform.position = new Vector2(Camera.current.transform.position.x - 8, Owl.transform.position.y);    
        else if (Owl.transform.position.x > Camera.current.transform.position.x + 8)
            Owl.transform.position = new Vector2(Camera.current.transform.position.x + 8, Owl.transform.position.y);
        else
            Owl.transform.position += transform.right * p1SidewaysInput * Time.deltaTime;


        if (Lemur.transform.position.x < Camera.current.transform.position.x - 8)
            Lemur.transform.position = new Vector2(Camera.current.transform.position.x - 8, Lemur.transform.position.y);
        else if (Lemur.transform.position.x >=Camera.current.transform.position.x + 8)
            Lemur.transform.position = new Vector2(Camera.current.transform.position.x + 8, Lemur.transform.position.y);
        else Lemur.transform.position += transform.right*p2SidewaysInput *Time.deltaTime;
      
    }

    void Jump()
    {
        if (p1JumpInput != 0 && OwlGrounded())
        {
            Owl.GetComponent<Rigidbody2D>().AddForce(Vector2.up * moveSettings.JumpVelocity, ForceMode2D.Impulse);
            // = new Vector2(player1Rigidbody.velocity.x, moveSettings.JumpVelocity);
        }
        if (p2JumpInput != 0 && LemurGrounded())
        {
            Lemur.GetComponent<Rigidbody2D>().AddForce(Vector2.up * moveSettings.JumpVelocity, ForceMode2D.Impulse);
            //player2Rigidbody.velocity = new Vector2(player2Rigidbody.velocity.x, moveSettings.JumpVelocity);
        }
    }

    bool LemurGrounded()
    {

        return Physics2D.Raycast(GameObject.FindGameObjectWithTag("Lemur").transform.position, Vector2.down, moveSettings.DistanceToGround, moveSettings.Ground);
    }

    bool OwlGrounded()
    {

        return Physics2D.Raycast(GameObject.FindGameObjectWithTag("Owl").transform.position, Vector2.down, moveSettings.DistanceToGround, moveSettings.Ground);
    }

    public void Spawn()
    {
        transform.position = spawnPoint.position;
    }

    // Update is called once per frame
    void Update () {
        //Debug.Log(p1JumpInput);

		if (Gamedata.Instance.Paused && gameObject.GetComponent<TimeReverse>() != null)
		return;

        GetPlayer1Input();
        GetPlayer2Input();
        Run();
        Jump();


    }

	void FixedUpdate(){

		if (Gamedata.Instance.Paused && gameObject.GetComponent<TimeReverse>() != null)
			return;


		//hae?????
		if (!Owl.GetComponent<Rigidbody2D>().isKinematic && !Lemur.GetComponent<Rigidbody2D>().isKinematic)
		{
			Run();
			Jump();
		}

		if (!Lemur.GetComponent<Rigidbody2D>().isKinematic)
		{
			Run();
			Jump();
		}

	}

	void Start(){

		trscript = GetComponent<TimeReverse> ();
		playerStats = GameObject.Find ("PlayerStats").GetComponent<Text> ();
		UpdateStats ();
		timereverse = false;
	}

	//fuer TimeReverse
	#region ITR implementation
	public void SaveTRObject ()
	{
		MyStatus status = new MyStatus();
		status.myPosition = transform.position;
		//status.myRotation = transform.rotation;, gibts doch nicht, oder?
		trscript.PushTRObject (status);
		Lemur.GetComponent<Rigidbody2D>().isKinematic = false;
		Owl.GetComponent<Rigidbody2D>().isKinematic = false;
	}

	public void LoadTRObject (TRObject trobject)
	{
		MyStatus newStatus = (MyStatus)trobject;
		transform.position = newStatus.myPosition;
		//transform.rotation = newStatus.myRotation;
		Owl.GetComponent<Rigidbody2D>().isKinematic = true;
		Lemur.GetComponent<Rigidbody2D>().isKinematic = true;
	}
	#endregion

	private class MyStatus: TRObject
	{
		public Vector2 myPosition;
		//evtl. brauche mehr
   	}

	void OnDeath()
	{
		Gamedata.Instance.Lives -= 1;
		UpdateStats ();

		//wenn schon verloren
		if(Gamedata.Instance.Lives == 0){
			Application.LoadLevel ("Verloren");

		}
		//sollte doch lieber das TimeReversal aufgerufen werden
		Spawn();
	}

	void OnCollisionEnter2D(Collision2D	other)
	{

		if (other.gameObject.tag == "Platform") {  //tag von der sich bewegenden Plattform


			//aktuelles Objekt(this. wichtig wenn mehrere Player), weilnicht static kann ich mehrere Player haben

			//             werde Child von dem was ich grad berühre

			//this.transform.parent = collision.transform;

			this.transform.SetParent(other.transform);  
			//                        das mit was ich colliediere wird Elternteil
			//              ist eine Methode
		}


		if(other.gameObject.tag == "Fallenemy"){
		Gamedata.Instance.Lives -= 1;
			UpdateStats ();

			//wenn schon verloren
			if(Gamedata.Instance.Lives == 0){
				Application.LoadLevel ("Verloren");

			}
	}

		if(other.gameObject.tag == "Enemy")
		{
			Enemy enemy = other.gameObject.GetComponent<Enemy>();
			BoxCollider2D col = other.gameObject.GetComponent<BoxCollider2D>();
			BoxCollider2D mycol = this.gameObject.GetComponent<BoxCollider2D>();

			if(enemy.invincible)
			{
				OnDeath();
			}
			else
				if(mycol.bounds.center.y - mycol.bounds.extents.y > col.bounds.center.y + 0.5f * 
					col.bounds.extents.y)
				{
					if(this.gameObject.tag == "Owl"){

						JumpedOnEnemy1(enemy.bumpSpeed);
					}
					if(this.gameObject.tag == "Lemur"){
						JumpedOnEnemy2(enemy.bumpSpeed);
					}

					enemy.OnDeath();
				}
			else
			{
				OnDeath();
			}
		}

	}


	void JumpedOnEnemy1(float bumpSpeed)
	{
		
		Owl.GetComponent<Rigidbody2D>().velocity = new Vector2 (Owl.GetComponent<Rigidbody2D>().velocity.x, bumpSpeed);

	}

	void JumpedOnEnemy2(float bumpSpeed)
	{

		Lemur.GetComponent<Rigidbody2D>().velocity = new Vector2 (Lemur.GetComponent<Rigidbody2D>().velocity.x, bumpSpeed);
	}


	void OnTriggerEnter2D(Collider2D other){


		if (other.tag == "Button1") {

			GameObject.FindGameObjectWithTag("Wand1").GetComponent<WandbewegeKnopf> ().bewegeHoch();
		}


		if (other.tag == "Affengrenze") {
			gameObject.GetComponent<fall_enemy> ().fallen ();
		}

		if (other.tag == "Deathzone") {
			
			OnDeathSpieler ();

		}

		if (other.tag == "Coin") {
			
			Gamedata.Instance.Score += 10;
			Destroy(other.gameObject);
			UpdateStats ();
		}

		if (other.tag == "Herz") {

			Gamedata.Instance.Lives += 1;
			Destroy(other.gameObject);
			UpdateStats ();
		}
	}

	//wenn Spieler Deathzone berührt, so wird der TimeReverse aktiviert

	void OnDeathSpieler(){
		Gamedata.Instance.Lives -= 1;
		UpdateStats ();

		//wenn schon verloren
		if(Gamedata.Instance.Lives == 0){
			Application.LoadLevel ("Verloren");

		}

		//TimeReverse wird aktiviert -> soll für Gegner, Platformen und Spieler gelten

		/*timereverse = true;
		int variable = 0;
		while(variable < 20){

		//	gameObject.GetComponent<TimeReverse>().Laden());
			variable++;
	}*/
	}
	public static void UpdateStats() 
	{ 
		playerStats.text = "Score: " + Gamedata.Instance.Score.ToString()
			 // ToString: zuerst ist e snur eine zahl, aber wir wollen einen string
			+ "\nLives: " + Gamedata.Instance.Lives.ToString(); 
		playerStats.text = "Score: " + Gamedata.Instance.Score.ToString()
			+ "\nLives: " +Gamedata.Instance.Lives.ToString(); 

	} 

    
}

[System.Serializable]
public class MoveSettings
{
    public float RunVelocity = 12;
    public float JumpVelocity = 1f;
    public float DistanceToGround = 0.5f;
    public LayerMask Ground;
}

[System.Serializable]
public class InputSettings
{
    public string PLAYER1_SIDEWAYS_AXIS = "Player1Horizontal";
    public string PLAYER1_JUMP_AXIS = "Player1Jump";
    public string PLAYER1_FLY_AXIS = "Player1Fly";
    public string PLAYER2_SIDEWAYS_AXIS = "Player2Horizontal";
    public string PLAYER2_JUMP_AXIS = "Player2Jump";
    public string PLAYER2_CROUCH_AXIS = "Player2Crouch";
}