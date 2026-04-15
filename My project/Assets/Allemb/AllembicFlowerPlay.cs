using UnityEngine;
using UnityEngine.Formats.Alembic.Importer;



public class AllembicFlowerPlay : MonoBehaviour
{

    private AlembicStreamPlayer alembicPlayer;

    [SerializeField] private float playbackSpeed = 1f;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = true;


    private float startTime;

    private float endTime;

    private bool isPlaying;

    private void Awake()
    {

        alembicPlayer = GetComponent<AlembicStreamPlayer>();

    }

    private void Start()
    {

        if (alembicPlayer == null)
        {

            enabled = false;

            return;

        }

        startTime = 0f;

        endTime = (float)alembicPlayer.EndTime;

        if (playOnStart)
        {

            alembicPlayer.CurrentTime = startTime;

            isPlaying = true;

        }

    }


    private void Update()
    {

        if (!isPlaying || alembicPlayer == null)
        {

            return;

        }

        float nextTime = alembicPlayer.CurrentTime + Time.deltaTime * playbackSpeed;

        if (nextTime > endTime)
        {

            if (loop)
            {

                nextTime = startTime;

            }
            else
            {

                nextTime = endTime;

                isPlaying = false;

            }
        }

        alembicPlayer.CurrentTime = nextTime;

        alembicPlayer.UpdateImmediately(nextTime);

    }


}
