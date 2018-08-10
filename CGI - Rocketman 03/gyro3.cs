    //Объявляем нужные блоки как глоб. переменные
    IMyRemoteControl RemCon;
    List<IMyGyro> gyroList;

    //координаты точки
    Vector3D Target = new Vector3D(100000, 0, 0);

    //Находим блоки, устанавливаем частоту обновления
    public Program()
    {
        gyroList = new List<IMyGyro>();
        GridTerminalSystem.GetBlocksOfType<IMyGyro>(gyroList);
        RemCon = GridTerminalSystem.GetBlockWithName("RemCon") as IMyRemoteControl;
        Runtime.UpdateFrequency = UpdateFrequency.Update1;
    }
    
    public void Main()
    {
        //Получаем и нормализуем вектор гравитации. Это наше направление "вниз" на планете.
        Vector3D GravityVector = RemCon.GetNaturalGravity();
        Vector3D GravNorm = Vector3D.Normalize(GravityVector);

        //вектор на точку
        Vector3D T = Vector3D.Normalize(Target - RemCon.GetPosition());

        //Рысканием прицеливаемся на точку Target.
        double tF = T.Dot(RemCon.WorldMatrix.Forward);
        double tL = T.Dot(RemCon.WorldMatrix.Left);
        float YawInput = -(float)Math.Atan2(tL, tF);
        
        //Получаем проекции вектора прицеливания на все три оси блока ДУ. 
        double gF = GravNorm.Dot(RemCon.WorldMatrix.Forward);
        double gL = GravNorm.Dot(RemCon.WorldMatrix.Left);
        double gU = GravNorm.Dot(RemCon.WorldMatrix.Up);
        
        //Получаем сигналы по тангажу и крены операцией atan2
        float RollInput = (float)Math.Atan2(gL, -gU);
        float PitchInput = -(float)Math.Atan2(gF, -gU);

        //для каждого гироскопа устанавливаем текущие значения по тангажу, крену, рысканию.
        foreach (IMyGyro gyro in gyroList)
        {
            gyro.GyroOverride = true;
            gyro.Yaw = YawInput;
            gyro.Roll = RollInput;
            gyro.Pitch = PitchInput;
        }
    }