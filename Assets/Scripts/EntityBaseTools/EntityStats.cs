using UnityEngine;

public class EntityStats : MonoBehaviour
{
    public long entityId = -1;
    public Object me;               //this game object

    public int level = 1;
    public float health = 1;
    public float stamina = 1;


    public BoxCollider boxCollider;     //objektcollider welches auf attacken reagiert

    public bool player = false;
    public string type = "ND";      //"Cat"
    public float maxHealth = 1;
    public float maxStamina;
    public float baseSpeed = 1;
    public float baseCritRate = 1;
    public float baseCritDamage = 1;
    public float defense = 1;
    //equipment[]

    //public ability        //main attack
    //public ability        //ability1
    //public ability        //ability2


    //List buffs                      //buffs change stats
    //list effects                    //effects do something

    

    public int inventoryMax = 0;
    //Inventory object
        //Item object
            //Type
            //Weight
            //Stats
            //Value (maybe calc)


    void SetStats(bool player, string type,float maxHealth, float maxStamina, float basespeed, float baseCritRate, float baseCritDamage, float defense)
    {
        this.player = player;
        this.type = type;
        this.maxHealth = maxHealth;
        this.maxStamina = maxStamina;
        this.baseSpeed = basespeed;
        this.baseCritRate = baseCritRate;
        this.baseCritDamage = baseCritDamage;
        this.defense = defense;
        health = maxHealth;
        stamina = maxStamina;
    }

    void GetStatsFromType()
    {
        //gets predefined basestats for this type
    }

    void InitialiseInventory()
    {
        //create inventory object
            //(list<ItemStack>) 
            //totalWeight
    }

    void RequestID() 
    {
        //idList.GetIDForEntity(me);                //soll long returnen
    }


    //calculate damage before actual health change
    void CalculateDamage() //take attack object
    {
        //random(critRate)
            //if yes crit = true

        //def muss ich mir noch überlegen wie genau, sollte attacken nur schwächen

        //if (crit) damage + damage * critD
        
        //add effect(attack.getEffect)      (this will ignore armor etc)
    }

    //return attack object
    void CalculateAttackStrength() 
    {
        //calculateEquipmentStats                       //(playerStats * LVmultiplyer + equipment)
        //calculateAttackStats(List<buffs>)             //bsp:
        //includeEffects(List<effects>)                 //bsp: DOT
        //createAttackObject(attack, critR, critD, buffs, effects)   //this gets requested by and attached to attacks
    }

    bool DestroyThisEntity()
    {
        if (player) { return false; }
        else
        {
            //remove id
            Destroy(me);
        }
        return true;
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        me = this.gameObject;
        if (player) { InitialiseInventory(); }
        RequestID();
    }

    // Update is called once per frame
    void Update()
    {
        if (player)
        {
            //prüfe eingabe und versuche abilities zu nutzen
        }
    }
}
