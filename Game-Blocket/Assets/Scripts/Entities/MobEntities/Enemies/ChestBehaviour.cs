using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static BlockData;

namespace Assets.Scripts.Entities.MobEntities.Enemies
{
    internal class ChestBehaviour : EnemyBehaviour
    {
        #region overwrittenFields
        [SerializeField]
        private int damage;
        public override int Damage { get => damage; set => damage = value; }
        [SerializeField]
        private int health;
        public override int Health { get => health; set => health = value; }
        [SerializeField]
        private int maxHealth;
        public override int MaxHealth { get => maxHealth; set => maxHealth = value; }
        [SerializeField]
        private int regeneration;
        public override int Regeneration { get => regeneration; set => regeneration = value; }

        private string deathanim;
        public override string deathAnimation { get => deathanim; set => deathanim = value; }

        [SerializeField]
        private Animator animator;
        public override Animator Animator { get => animator; set => animator = value; }
        [SerializeField]
        private uint mobId;
        public override uint MobID { get => mobId; set => mobId = value; }

        [SerializeField]
        private List<BlockDropAble> drops;
        public override List<BlockDropAble> Drops { get => drops; set => drops = value; }
        #endregion
    }
}
