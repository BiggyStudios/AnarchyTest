using System.Numerics;

using Vertex;
using Vertex.Input;
using Vertex.Physics;

namespace AnarchyTest
{
    /// <summary>
    /// Rivali, in first person: walks with WASD, hurries with Shift, crouches with Ctrl and looks around with the
    /// mouse. Slow on purpose: this is somewhere to creep through, not run round. Click in the window to take the
    /// mouse, Escape to let it go.
    /// </summary>
    public class Player : Component
    {
        [Header("Moving")]
        public float WalkSpeed { get; set; } = 1.8f;
        public float RunSpeed { get; set; } = 3.4f;
        public float CrouchSpeed { get; set; } = 0.9f;
        public float Gravity { get; set; } = 18f;

        [Header("Looking")]
        public float MouseSensitivity { get; set; } = 0.11f;

        /// <summary>How high the eyes are standing, and crouched.</summary>
        public float EyeHeight { get; set; } = 1.62f;
        public float CrouchEyeHeight { get; set; } = 0.95f;

        /// <summary>How far the head rises and falls with each step, in metres.</summary>
        [Range(0f, 0.1f)]
        public float HeadBob { get; set; } = 0.035f;

        /// <summary>The camera's object, under this one: it tips up and down while the whole body turns left and right.</summary>
        public GameObject? Head { get; set; }

        /// <summary>Whether the player can move and look: off while something else has hold of them (a scripted moment).</summary>
        public bool CanMove { get; set; } = true;

        public bool Crouching { get; private set; }

        private CharacterController? _character;
        private float _pitch;
        private float _eyes;
        private float _stride;

        protected override void Start()
        {
            _character = GetComponent<CharacterController>();
            _eyes = EyeHeight;
            if (Head != null) _pitch = Head.Transform.EulerAngles.X;
        }

        protected override void Update()
        {
            if (_character == null) return;

            if (InputSystem.GetMouseButtonDown(MouseButton.Left)) InputSystem.CursorLocked = true;
            // Not while reading: there Escape puts the page away.
            if (CanMove && InputSystem.GetKeyDown(Key.Escape)) InputSystem.CursorLocked = false;

            if (CanMove && InputSystem.CursorLocked) Look();
            Walk(_character);
            PlaceHead(_character);
        }

        private void Look()
        {
            Vector2 mouse = InputSystem.MouseDelta * MouseSensitivity;
            Transform.EulerAngles += new Vector3(0f, -mouse.X, 0f);
            _pitch = Math.Clamp(_pitch - mouse.Y, -85f, 85f);
        }

        private void Walk(CharacterController character)
        {
            var move = Vector3.Zero;
            if (CanMove)
            {
                float forward = (InputSystem.GetKey(Key.W) ? 1f : 0f) - (InputSystem.GetKey(Key.S) ? 1f : 0f);
                float right = (InputSystem.GetKey(Key.D) ? 1f : 0f) - (InputSystem.GetKey(Key.A) ? 1f : 0f);
                move = Transform.Forward * forward + Transform.Right * right;
                move.Y = 0f;
                if (move.LengthSquared() > 1f) move = Vector3.Normalize(move);
            }

            Crouching = CanMove && (InputSystem.GetKey(Key.ControlLeft) || InputSystem.GetKey(Key.C));
            float speed = Crouching ? CrouchSpeed : InputSystem.GetKey(Key.ShiftLeft) ? RunSpeed : WalkSpeed;
            move *= speed;

            float fall = character.IsGrounded ? -1f : character.Velocity.Y - Gravity * Time.DeltaTime;
            character.Velocity = new Vector3(move.X, fall, move.Z);
            character.Move(Time.DeltaTime);

            // Steps come with the distance walked, so the head bobs faster when hurrying.
            if (character.IsGrounded) _stride += move.Length() * Time.DeltaTime * 1.9f;
        }

        private void PlaceHead(CharacterController character)
        {
            if (Head == null) return;

            // Crouching lowers the eyes smoothly rather than at once.
            float target = Crouching ? CrouchEyeHeight : EyeHeight;
            _eyes += (target - _eyes) * MathF.Min(1f, Time.DeltaTime * 10f);

            bool walking = new Vector2(character.Velocity.X, character.Velocity.Z).LengthSquared() > 0.01f;
            float bob = walking ? MathF.Sin(_stride * MathF.PI) * HeadBob : 0f;
            float sway = walking ? MathF.Cos(_stride * MathF.PI * 0.5f) * HeadBob * 0.5f : 0f;

            Head.Transform.Position = new Vector3(sway, _eyes + bob, 0f);
            Head.Transform.EulerAngles = new Vector3(_pitch, 0f, 0f);
        }
    }
}
