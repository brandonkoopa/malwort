# This script is based on the default CharacterBody2D template. Not much interesting happening here.
extends CharacterBody2D

const SPEED_MIN = 300.0
const SPEED_MAX = 400.0
const ACCEL = 50.0
const JUMP_VELOCITY = -450.0
const MAX_FALL_SPEED = 900.0
const COYOTE_TIME: float = .1
const SHORT_HOP: float = .5

var gravity: int = ProjectSettings.get_setting("physics/2d/default_gravity")
var animation: String

var reset_position: Vector2
# Indicates that the player has an event happening and can't be controlled.
var event: bool

var abilities: Array[StringName]
var double_jump: bool
var prev_on_floor: bool
var airtime: float = 0
var speed: float = SPEED_MIN

# Arrow shooting variables
var arrow_scene = preload("res://SampleProject/Objects/Arrow.tscn")
var can_shoot = true
var shoot_cooldown = 0.5  # Half second cooldown between shots
var x_key_was_pressed = false  # Track X key state for single press detection

func _ready() -> void:
	# Set up collision detection
	collision_layer = 1  # Set to layer 1 for player
	collision_mask = 1   # Collide with layer 1 (enemies)
	
	on_enter()

func _physics_process(delta: float) -> void:
	if event:
		return
	
	if not is_on_floor():
		velocity.y = min(velocity.y + gravity * delta, MAX_FALL_SPEED)
		airtime += delta
	elif not prev_on_floor and &"double_jump" in abilities:
		# Some simple double jump implementation.
		double_jump = true
		airtime = 0
	
	var on_floor_ct: bool = is_on_floor() or airtime < COYOTE_TIME
	if Input.is_action_just_pressed("ui_accept") and (on_floor_ct or double_jump):
		if not on_floor_ct:
			double_jump = false
		
		if Input.is_action_pressed("ui_down"):
			position.y += 8
		else:
			velocity.y = JUMP_VELOCITY
	
	if Input.is_action_just_released("ui_accept"):
		if not is_on_floor() and velocity.y < 0:
			velocity.y = min(0, velocity.y - JUMP_VELOCITY * SHORT_HOP)
			
	
	if is_on_wall():
		speed = SPEED_MIN
	
	var direction := Input.get_axis("ui_left", "ui_right")
	if direction:
		speed = min(SPEED_MAX, speed + ACCEL * delta)
		velocity.x = direction * speed
	else:
		velocity.x = move_toward(velocity.x, 0, SPEED_MIN)
		speed = SPEED_MIN

	prev_on_floor = is_on_floor()
	move_and_slide()
	
	var new_animation = &"Idle"
	if velocity.y < 0:
		new_animation = &"Jump"
	elif velocity.y >= 0 and not is_on_floor():
		new_animation = &"Fall"
	elif absf(velocity.x) > 1:
		new_animation = &"Run"
	
	if new_animation != animation:
		animation = new_animation
		$AnimationPlayer.play(new_animation)
	
	if velocity.x > 1:
		$Sprite2D.flip_h = false
	elif velocity.x < -1:
		$Sprite2D.flip_h = true
	
	# Handle arrow shooting
	handle_shooting()

func handle_shooting():
	# Check for shoot action (X key) or direct X key press
	var x_key_pressed = Input.is_key_pressed(KEY_X)
	var shoot_action_pressed = Input.is_action_just_pressed("shoot")
	
	# Detect X key just pressed (not held)
	var x_key_just_pressed = x_key_pressed and not x_key_was_pressed
	x_key_was_pressed = x_key_pressed
	
	if (shoot_action_pressed or x_key_just_pressed) and can_shoot:
		print("Shooting arrow!")  # Debug output
		shoot_arrow()
		can_shoot = false
		# Start cooldown timer
		await get_tree().create_timer(shoot_cooldown).timeout
		can_shoot = true
	elif (shoot_action_pressed or x_key_just_pressed) and not can_shoot:
		print("Shoot action pressed but on cooldown")  # Debug output
	
	# Debug: Check if X key is being pressed directly
	if x_key_pressed:
		print("X key is being pressed")
	
	# Debug: Check if any key is being pressed
	if Input.is_anything_pressed():
		print("Some input is being detected")

func shoot_arrow():
	# Create arrow instance
	var arrow = arrow_scene.instantiate()
	
	# Set arrow position (slightly in front of player)
	var arrow_direction = 1 if not $Sprite2D.flip_h else -1
	var spawn_offset = Vector2(30 * arrow_direction, -10)  # Adjust these values as needed
	arrow.global_position = global_position + spawn_offset
	
	# Set arrow direction
	arrow.direction = arrow_direction
	
	# Add arrow to the current scene
	get_parent().add_child(arrow)
	
	print("Arrow spawned at: ", arrow.global_position, " with direction: ", arrow_direction)  # Debug output

func kill():
	# Player dies, reset the position to the entrance.
	position = reset_position
	Game.get_singleton().load_room(MetSys.get_current_room_name())

func on_enter():
	# Position for kill system. Assigned when entering new room (see Game.gd).
	reset_position = position
