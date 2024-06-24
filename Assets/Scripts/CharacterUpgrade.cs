using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "CharacterUpgrade", menuName = "ScriptableObjects/Character Upgrade")]
public class PlayerUpgrade : ScriptableObject
{
    public enum Stats
    {
        Health,
        Damage,
        MovementRadius,
        Range
    }
    
    [SerializeField] private Stats _stat;
    [SerializeField, Tooltip("Valor em multiplicador")] private float _value;
    [SerializeField] private Texture2D _image;
    [SerializeField] private string _name;
    [SerializeField] private string _description;
    [SerializeField] private int chance;
    
    public Stats Stat => _stat;
    public string Name => _name;
    public float Value => _value;
    public Texture2D Image => _image;
    public string Description => _description;
    
    public int Chance => chance;
    
    public VisualElement Card (VisualTreeAsset template)
    {
        //clona o template do card
        var upgradeCard = template.CloneTree();
        
        //preenche o card com as informações do ataque
        upgradeCard.Q<Label>("Title").text = Name;
        upgradeCard.Q<Label>("Image").style.backgroundImage = Image;
        upgradeCard.Q<Label>("Description").text = Description;
        
        //retorna o card
        return upgradeCard;
    }
}
