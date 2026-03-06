using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

[Serializable]
public struct EnemyBatchMessage : NetworkMessage
{
    public List<EnemyCommand> tempCommands;

    // Constructor
    public EnemyBatchMessage(List<EnemyCommand> tempCommands)
    {
        this.tempCommands = tempCommands;
    }
    public void Serialize(NetworkWriter writer)
    {
        writer.WriteInt(tempCommands.Count);
        foreach (var cmd in tempCommands)
        {
            writer.WriteInt(cmd.enemyId);
            writer.WriteInt(cmd.spawnerId);
            writer.WriteByte((byte)cmd.commandType);
            writer.WriteVector3(cmd.position);
            writer.WriteVector3(cmd.direction);
            writer.WriteFloat(cmd.distance);
        }
    }

    public void Deserialize(NetworkReader reader)
    {
        // This is called by Mirror INTERNALLY before the handler fires
        tempCommands.Clear();
        int count = reader.ReadInt();

        for (int i = 0; i < count; i++)
        {
            tempCommands.Add(new EnemyCommand
            {
                enemyId = reader.ReadInt(),
                spawnerId = reader.ReadInt(),
                commandType = (EnemyCommandType)reader.ReadByte(),
                position = reader.ReadVector3(),
                direction = reader.ReadVector3(),
                distance = reader.ReadFloat()
            });
        }
    }
}
