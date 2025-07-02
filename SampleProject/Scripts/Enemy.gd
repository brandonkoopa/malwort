extends CharacterBody2D

const SPEED = 100.0
const GRAVITY = 980
const DETECTION_RANGE = 150.0  # How far the enemy can "see" the player

var direction = 1  # 1 for right, -1 for left
var health = 3
var is_dead = false
var player = null
var flip_default_direction = true  # Set to true if your sprite faces left by default
var is_tracking_player = false
var last_direction_change = 0.0
var direction_change_cooldown = 0.5  # Minimum time between direction changes
var direction_change_buffer = 0.2    # Extra buffer to prevent rapid flipping

func _ready():
	# Start the patrol timer
	$PatrolTimer.start()
	# Find the player
	player = get_tree().get_first_node_in_group("player")
	if not player:
		# Try to find player by metadata
		for node in get_tree().get_nodes_in_group(""):
			if node.has_meta("player"):
				player = node
				break
	
	# Set up collision detection
	collision_layer = 1  # Set to layer 1 for enemies
	collision_mask = 3   # Collide with layer 1 (other enemies) and layer 2 (projectiles) - binary 11
	
	# Automatically adjust collision shape to match sprite size
	adjust_collision_to_sprite()

func adjust_collision_to_sprite():
	var sprite = $Sprite2D
	var collision_shape = $CollisionShape2D
	
	if sprite.texture and collision_shape.shape:
		# Get the actual texture size
		var texture_size = sprite.texture.get_size()
		
		# Apply sprite scaling (use absolute values to ignore negative scale for flipping)
		var final_size = texture_size * sprite.scale.abs()
		
		# Set the collision shape size to match the sprite
		collision_shape.shape.size = final_size
		
		# Center the collision shape on the sprite
		collision_shape.position = Vector2.ZERO
		
		print("Enemy collision adjusted to: ", final_size)

func _physics_process(delta):
	if is_dead:
		return
	
	# Apply gravity
	if not is_on_floor():
		velocity.y += GRAVITY * delta
	
	# Check if player is nearby and update direction
	var player_in_range = player and is_player_in_range()
	if player_in_range and not is_tracking_player:
		is_tracking_player = true
		update_direction_towards_player()
	elif not player_in_range and is_tracking_player:
		is_tracking_player = false
	
	# Only update direction towards player if we're tracking them
	if is_tracking_player and player_in_range:
		update_direction_towards_player()
	
	# Move horizontally
	velocity.x = direction * SPEED
	
	# Check for walls and turn around (only if not tracking player or if we've been stuck)
	var now = Time.get_ticks_msec() / 1000.0
	if is_on_wall() and (not is_tracking_player or (now - last_direction_change > direction_change_cooldown + direction_change_buffer)):
		flip_direction()
	
	# Check for edges (optional - uncomment if you want enemies to turn at edges)
	# if not is_on_floor() and is_on_wall():
	#     flip_direction()
	
	move_and_slide()
	
	# Update sprite direction (with flip_default_direction option)
	var should_flip = (direction < 0) != flip_default_direction
	$Sprite2D.flip_h = should_flip

func is_player_in_range():
	if not player:
		return false
	var distance = global_position.distance_to(player.global_position)
	return distance < DETECTION_RANGE

func update_direction_towards_player():
	if not player:
		return
	
	# Determine which direction the player is relative to the enemy
	var player_direction = 1 if player.global_position.x > global_position.x else -1
	
	# Only change direction if the player is on a different side and enough time has passed
	var now = Time.get_ticks_msec() / 1000.0
	if player_direction != direction and now - last_direction_change > direction_change_cooldown + direction_change_buffer:
		direction = player_direction
		last_direction_change = now
		$PatrolTimer.start()  # Restart timer when changing direction

func flip_direction():
	var now = Time.get_ticks_msec() / 1000.0
	if now - last_direction_change > direction_change_cooldown + direction_change_buffer:
		direction *= -1
		last_direction_change = now
		$PatrolTimer.start()  # Restart timer when changing direction

func _on_patrol_timer_timeout():
	# Change direction periodically (only if not tracking player)
	if not is_player_in_range():
		# Check if player is behind and close to entering detection range
		if player:
			var player_offset = player.global_position.x - global_position.x
			var player_behind = (direction == 1 and player_offset < 0) or (direction == -1 and player_offset > 0)
			var player_close = abs(player_offset) < DETECTION_RANGE * 1.2  # 20% buffer
			if player_behind and player_close:
				return  # Don't flip if player is about to enter detection range from behind
		flip_direction()

func take_damage(amount = 1):
	if is_dead:
		return
	
	health -= amount
	if health <= 0:
		die()

func die():
	is_dead = true
	# You can add death animation here
	# For now, just make it invisible and disable collision
	$Sprite2D.visible = false
	$CollisionShape2D.set_deferred("disabled", true)
	
	# Enemy stays dead permanently - no respawn
	# await get_tree().create_timer(5.0).timeout
	# respawn()

func _on_body_entered(body):
	if body.has_method("kill") and not is_dead:
		# Damage the player
		body.kill() 
