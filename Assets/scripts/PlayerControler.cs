using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerControler : MonoBehaviour
{
	public float speed;
	public Text countText;
	public Text winText;
	public Text errorText;
	private Rigidbody rb;
	private int count;


	private YTilt tilt_x;
	private YTilt tilt_z;
	private YCompass compass;
	private double _x;
	private double _z;
	private double _compass;

	void Start ()
	{
		rb = GetComponent<Rigidbody> ();
		count = 0;
		setCountText ();
		winText.text = "";
		errorText.text = "";
		YoctopuceInitialisation ();
	}


	void FixedUpdate ()
	{
		//OriginalFixedUpdate ();
		//WrongApproach ();
		EfficientApproach ();
	}



	void OnTriggerEnter (Collider other)
	{
		if (other.gameObject.CompareTag ("PickUp")) {
			other.gameObject.SetActive (false);
			count++;
			setCountText ();
			if (count >= 12) {
				winText.text = "You Win!";
			}
		}
	}

	void setCountText ()
	{
		countText.text = "Count: " + count.ToString ();
	}


	void YoctopuceInitialisation ()
	{
		Debug.Log ("Use Yoctopuce Lib " + YAPI.GetAPIVersion ());
		string errmsg = "";
		int res = YAPI.RegisterHub ("usb", ref errmsg);
		if (res != YAPI.SUCCESS) {
			Debug.Log ("error with RegisterHub:" + errmsg);
			errorText.text = errmsg;
			return;
		}
		YModule module = YModule.FirstModule ();
		while (module != null) {
			string product = module.get_productName ();
			if (product == "Yocto-3D" || product == "Yocto-3D-V2") {
				Debug.Log ("Use " + product + " " + module.get_serialNumber ());
				break;
			}
			module = module.nextModule ();
		}
		if (module == null) {
			errorText.text = "No Yocto-3D or Yocto-3D-V2 found";
			return;
		}
		string serial = module.get_serialNumber ();
		tilt_x = YTilt.FindTilt (serial + ".tilt1");
		tilt_z = YTilt.FindTilt (serial + ".tilt2");

		tilt_x.registerValueCallback (TiltCallbackX);
		tilt_z.registerValueCallback (TiltCallbackZ);
	}

	void OriginalFixedUpdate ()
	{
		float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");
		Vector3 movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);
		rb.AddForce (movement * speed);
	}

	void WrongApproach ()
	{
		ulong tm_start = YAPI.GetTickCount ();
		double x = tilt_x.get_currentValue ();
		double z = -tilt_z.get_currentValue ();
		ulong tm_stop = YAPI.GetTickCount ();
		Debug.Log (string.Format ("Get values (r{0:f} p{1:f} took {2:d}ms", x, z, tm_stop - tm_start));
		Vector3 movement = new Vector3 ((float)x, 0.0f, (float)z);
		rb.AddForce (movement * speed);
	}

	void EfficientApproach ()
	{
		ulong tm_start = YAPI.GetTickCount ();
		string errmsg = "";
		int res = YAPI.HandleEvents (ref errmsg);
		if (res != YAPI.SUCCESS) {
			errorText.text = errmsg;
			return;
		}
		ulong tm_stop = YAPI.GetTickCount ();
		Debug.Log (string.Format ("Get values (r{0:f} p{1:f} took {2:d}ms", _x, _z, tm_stop - tm_start));
		Vector3 movement = new Vector3 ((float)_x, 0.0f, (float)_z);
		rb.AddForce (movement * speed);
	}

	void TiltCallbackX (YTilt sensor, string value)
	{
		_x = double.Parse (value);
	}

	void TiltCallbackZ (YTilt sensor, string value)
	{
		_z = -double.Parse (value);
	}



}
