using UnityEngine;
using System.Collections;


public enum ElementsTypeEnum
{
    Empty, //пустой блок
    StandardElement, //стандартный блок в который вкладывается элемент
    SpecElement,
    ImmortalWall, //неразрушаемая стена
    CrushableWall, //разрушаемый блок, который предварительно нужно разбить
    MediumFlask, //фласка средняя
    SmallFlask, //фласка малая
    BigFlask,//фласка большая
    SeedBarrel,//бочка собирающая элементы
    Drop,//сбрасваемый элемент
    WildPlant, //дикое растение
    MagicBush, //магический куст
}//Типы элементов

public enum ElementsShapeEnum
{
    Empty, //пустой элемент
    Carrot, //морковь
    Watermelon, //арбуз
    Apple, //яблоко
    Banana, //банан
    Wall, //стандартная стена
    MediumFlask, //фласка средняя
    SmallFlask, //фласка малая
    Strawberry, //клубника
    Orange,
    Camomile, //ромашка
    Plum, //слива
    BigFlask,//фласка большая
    Mushroom,//гриб
    SeedBarrel,//бочка
    Bush,//куст
    Brick,//кирпич
    WildPlant, //дикое растение
    MagicBush, //магический куст
    MagicFruit, //магический фрукт
}//Внешние виды элементов

public enum BehindElementsTypeEnum
{
    Empty, //пустой 
    Grass, //стандартный элемент
    Dirt, //грязь
}//Типы элементов позади

public enum BehindElementsShapeEnum
{
    Empty, //пустой элемент
    Grass, //трава
    Dirt, //грязь
}//Внешние виды элементов позади

public enum BlockingElementsTypeEnum
{
    Empty, //пустой элемент
    Liana, //стандарртный блокирующий элемент
    Spread, //распространяемый
}//Типы блокирующих элементов

public enum BlockingElementsShapeEnum
{
    Empty, //пустой элемент
    Liana, //лиана   
    Web, //паутина
}//Внешние виды блокирующих элементов


public enum AllShapeEnum
{
    Empty, //пустой элемент
    Carrot, //морковь
    Watermelon, //арбуз
    Apple, //яблоко
    Banana, //банан
    Wall, //стандартная стена
    MediumFlask, //бомба
    SmallFlask, //динамит
    Liana, //лиана
    Strawberry, //клубника
    Orange,
    Camomile, //ромашка
    Plum, //слива
    BigFlask,//фласка большая
    Grass, //трава
    Dirt, //грязь
    Mushroom,//гриб
    SeedBarrel,//бочка
    Bush,//куст
    Brick,//кирпич
    Web, //паутина
    WildPlant, //дикое растение
    MagicBush, //магический куст
    MagicFruit, //магический фрукт
}//Внешние виды элементов

public enum BlockTypeEnum
{
    Empty, //пустой блок
    StandardBlock, //стандартный блок
    Sliding //скользящий
}//Типы блоков

public enum InstrumentsEnum
{
    Empty, //пустой элемент
    Shovel, //лопата
    Hoe, //мотыга
    Vortex, //вихрь
    Repainting, //перекраска
}//инсструменты

public enum HitTypeEnum
{
    Empty,
    StandartHit, //прямой удар по элементу
    HitFromNearbyElement, //удар от соседнего элемента
    Explosion, //взрыв
    DoubleClick, //двойной клик по элементу
    Drop, //сброс
    Instrument, //удар от инструмента
    //DestroyElement, //удар от уничтоженного элемента
}//Типы ударов по блоку

public enum CollectionTypesEnum {
    Element,
    BlockingElement,
}//Типы коллекций

public enum DirectionEnum
{
    Empty,
    Left,
    Right,
    Up,
    Down,
    All
}//направление

public enum BorderEnum
{
    Empty,
    Line,
    InAngle,
    OutAngle
}//типы обвотки

public enum SmoothEnum
{
    InArcWithSlowdown,
    InArc,
    InLineWithSlowdown,
    InLineWithOneSpeed,
    InLineWithAcceleration
}//тип сглаженного перемещения

public enum HelpEnum
{
    Empty,
    Gnome,
    GnomeStandardElement,
    Score,
    Tasks,    
    SuperBonus,
    Instruments,
    Shop,
    Hoe,
    Vortex, //вихрь
    Repainting, //перекраска
    Line4,//линия из 4 элементов
    Line5,
    Line6,
    Gift,
    OptionalLvl,
    Lifes,
    DailyGift,
    DropBlock,
    StartRegion1,
    StartRegion2,
    Spider,
    WildPlantAndSpiderTogether,
    MagicFruitHelp,

}//тип сглаженного перемещения

public enum SoundsEnum
{
    EmptySound,
    CreateElement,
    Create_liana,
    Spread_liana,
    Create_web,
    Create_wildplant,
    Destroy_web,
    Destroy_wildplant,
    Destroy_liana,
    Destroy_liana_2,
    Destroy_liana_3,
    Preparation_wildplant,
    DestroyElement_1,
    DestroyElement_2,
    DestroyElement_3,
    DestroyElement_4,
    DestroyElement_5,
    Destroy_brick,
    CreateBonus,
    Magic,
    Bite,
    LeafRustling,
    Boom,
    Boom_mini_1,
    Boom_mini_2,
    Boom_mini_3,
    Boom_mini_4,
    Boom_big,
    SuperBonusRocket,
    SuperBonusActiveted,
    CollectElement,
    Applause,
    ClickButton,
    Victory,
    Defeat,
    Star,
    Coin,
    Ring_1,
    Ring_2,
    Laser,
    Card,
    Wind,
    Wind_active,
    Shovel,
    LawnMowerStart,
    LawnMowerWork,
    Repainting,
    Repainting_ring,
    StartGame,
    AddMove,
    Dirt_create,
    Dirt_destroy,
    Dirt_swelling,
    Closed_chest,
    Zero_moves,
    Two_moves,
    Life_add,
    Life_sub,
    Hit_1,
    Hit_2,
    Hit_3,
    Hit_4,
    Hit_5,
    Spider_1,
    Spider_2,
    SeedBarrel_collect,

}//тип звуков

public enum PSEnum
{
    EmptyPS,
    PSCollectAll,
    PSCollect,
    PSMagicalTail,
    PSSelect,
    PSAddPowerSuperBonus,
    PSBeatsSuperBonus,
    PSDirt,
    PSDirtNextAction,//эффект подсветки грязи которая будет ходит в следующем ходу
    PSWeb,
    PSLiana,
    PSWildPlantNextAction,
    PSRocket,
    PSAddSuperBonusFromLevels,
    PSSuperBonusActiveted,
    PSSelectTargetBlock,
    PSSelectTargetBlockBlue,
    PSMoveElement,
    PSSmallFlask,
    PSMediumFlask,
    PSBigFlask,
    PSBoom

}//тип эфектов

public enum VideoForFeeEnum
{
    ForCoin,
    ForMove,
    ForLive,
    ForDailyGift,
}//тип видео за вознаграждение

public enum SpritesEnum
{
    Empty,
    Coin,
    Move,
    Life,
    Star,
    //Daily_Gift,
    Gift_Box,
    Gift_Box_Open,
    Button_Shop,
    Button_Shop_Close,
    Targets_panel,
    Tasks_panel,

}//спрайты

public enum ArmMovementEnum
{
    Empty,
    Up,
    Down,
    Left,
    Right,
    All_directions,
    Double_click

}//движения руки-подсказки