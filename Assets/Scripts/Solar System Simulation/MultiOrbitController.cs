using UnityEngine;

public class MultiOrbitController : MonoBehaviour
{
    public Transform centerOfMass;
    public int numberOfPlanets = 3; // Cantidad de planetas
    public float minRadius = 5f;
    public float maxRadius = 20f;
    public float minMass = 0.1f;
    public float maxMass = 1f;

    private Planet[] planets;

    [System.Serializable]
    public class Planet
    {
        public Transform planetTransform;
        public float mass;
        public float semiMajorAxis;
        public float eccentricity;
        public float orbitalInclination;
        public float orbitalSpeed;
        private float orbitalAngle;

        public Vector3 CalculatePosition(Vector3 center, float time)
        {
            float meanAnomaly = orbitalAngle + orbitalSpeed * time;
            float eccentricAnomaly = CalculateEccentricAnomaly(meanAnomaly);
            float trueAnomaly = CalculateTrueAnomaly(eccentricAnomaly);
            float distance = semiMajorAxis * (1 - eccentricity * Mathf.Cos(eccentricAnomaly));

            Vector3 orbitalPosition = new Vector3(
                Mathf.Cos(trueAnomaly),
                0f,
                Mathf.Sin(trueAnomaly)
            );

            Quaternion rotation = Quaternion.Euler(0, orbitalInclination, 0);
            return center + rotation * (distance * orbitalPosition);
        }

        private float CalculateEccentricAnomaly(float meanAnomaly)
        {
            float eccentricAnomaly = meanAnomaly;
            float delta;

            do
            {
                delta = eccentricAnomaly - eccentricity * Mathf.Sin(eccentricAnomaly) - meanAnomaly;
                eccentricAnomaly -= delta / (1 - eccentricity * Mathf.Cos(eccentricAnomaly));
            } while (Mathf.Abs(delta) > 1e-6f);

            return eccentricAnomaly;
        }

        private float CalculateTrueAnomaly(float eccentricAnomaly)
        {
            float trueAnomaly = 2 * Mathf.Atan2(Mathf.Sqrt(1 + eccentricity) * Mathf.Sin(eccentricAnomaly / 2),
                                                Mathf.Sqrt(1 - eccentricity) * Mathf.Cos(eccentricAnomaly / 2));
            return trueAnomaly;
        }
    }

    void Start()
    {
        GenerateRandomPlanets();
    }

    void Update()
    {
        foreach (Planet planet in planets)
        {
            Vector3 newPosition = planet.CalculatePosition(centerOfMass.position, Time.time);
            planet.planetTransform.position = newPosition;
            planet.planetTransform.LookAt(centerOfMass);
        }
    }

    void GenerateRandomPlanets()
    {
        planets = new Planet[numberOfPlanets];
        for (int i = 0; i < numberOfPlanets; i++)
        {
            float randomRadius = Random.Range(minRadius, maxRadius);
            float randomMass = Random.Range(minMass, maxMass);

            planets[i] = new Planet
            {
                planetTransform = new GameObject("Planet" + i).transform,
                mass = randomMass,
                semiMajorAxis = randomRadius,
                eccentricity = Random.Range(0f, 0.5f),
                orbitalInclination = Random.Range(0f, 30f),
                orbitalSpeed = Random.Range(0.1f, 0.5f)
            };
        }
    }
}