var speed = 6.0;

public var cam : Transform;
private var controller : CharacterController;

function Start()
{
	controller = GetComponent(CharacterController);
}

function Update() 
{
	moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
	moveDirection = cam.TransformDirection(moveDirection);
	moveDirection *= speed;
	controller.Move(moveDirection * Time.deltaTime);
}

@script RequireComponent(CharacterController)