extends CharacterBody2D

const SPEED = 400.0
const GRAVITY = 0  # Arrows don't fall

var direction = 1  # 1 for right, -1 for left
var damage = 1

func _ready():
	# Connect the lifetime timer
	$LifetimeTimer.timeout.connect(_on_lifetime_timer_timeout)
	
	# Set up collision detection
	collision_layer = 2  # Set to layer 2 for projectiles
	collision_mask = 1   # Collide with layer 1 (enemies)
	
	# Set the arrow direction based on player facing
	if direction < 0:
		$Sprite2D.flip_h = true

func _physics_process(delta):
	# Move the arrow
	velocity.x = direction * SPEED
	velocity.y = 0  # No vertical movement for now
	
	move_and_slide()
	
	# Check for collisions with enemies
	for i in get_slide_collision_count():
		var collision = get_slide_collision(i)
		var collider = collision.get_collider()
		
		if collider and collider.has_method("take_damage"):
			# Hit an enemy!
			print("Arrow hit enemy: ", collider.name)  # Debug output
			collider.take_damage(damage)
			queue_free()  # Destroy the arrow
			return

func _on_lifetime_timer_timeout():
	# Arrow disappears after lifetime
	queue_free() 
