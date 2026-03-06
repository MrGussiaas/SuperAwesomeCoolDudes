using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyBatchReceiver : MonoBehaviour
{
    private Dictionary<int, Enemy> activeEnemies = new();

    void Start()
    {
        // Use a lambda to 'bridge' the message to your reader logic
        NetworkClient.RegisterHandler<EnemyBatchMessage>(OnBatchReceived);  
    }

    // Your actual logic method (no longer a Mirror-registered method)
    void OnBatchReceived(EnemyBatchMessage msg)
    {
      for (int i = 0; i < msg.tempCommands.Count; i++)
        {
            EnemyCommand cmd = msg.tempCommands[i];
            ProcessCommand(cmd.enemyId, cmd.commandType, cmd.position, cmd.direction, cmd.distance);
        }
    }
    private void ProcessCommand(int id, EnemyCommandType type, Vector3 pos, Vector3 dir, float dist)
    {
        VisualEnemy visualEnemy = VisualEnemyManager.Instance.GetEnemyById(id);
        if(visualEnemy == null){
            return;
        }
        switch (type)
        {
            case EnemyCommandType.MOVE_FORWARD :
                {
                    
                    visualEnemy.MoveForward(dir, dist);
                    break;
                }
            case EnemyCommandType.MOVE_FORWARD_NO_DIR :
                {
                    visualEnemy.MoveForward(dist);
                    break;
                }
            case EnemyCommandType.FINISH_MOVEMENT :
                {
                    visualEnemy.FinishMovement(pos, false);
                    break;
                }
            case EnemyCommandType.FINISH_MOVEMENT_CANCELLED :
                {
                    visualEnemy.FinishMovement(pos, true);
                    break;
                }
            case EnemyCommandType.START_ROTATION :
                {
                    visualEnemy.RotateTo(dir);
                    break;        
                }
            case EnemyCommandType.FINISH_ROTATION :
                {
                    visualEnemy.FinishRotation(dir);
                    break;        
                }
            case EnemyCommandType.DESTROY :
                {
                    bool existed = VisualEnemyManager.Instance.DestroyVisualEnemy(id);
                    
                    IVisualExplosion explosion = VisualExplosionManager.Instance.GetPooledExplosion();
                    explosion.StartExplosion(pos);
                    break;
                }

        }
    }
}
