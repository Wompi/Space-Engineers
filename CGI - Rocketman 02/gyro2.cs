    //Объявляем нужные блоки как глоб. переменные
    IMyRemoteControl RemCon;
    List<IMyGyro> gyroList;

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

        //Получаем проекции вектора прицеливания на все три оси блока ДУ. 
        double gF = GravNorm.Dot(RemCon.WorldMatrix.Forward);
        double gL = GravNorm.Dot(RemCon.WorldMatrix.Left);
        double gU = GravNorm.Dot(RemCon.WorldMatrix.Up);
        
        //Получаем сигналы по тангажу и крены операцией atan2
        float RollInput = (float)Math.Atan2(gL, -gU);
        float PitchInput = -(float)Math.Atan2(gF, -gU);

        //На рыскание просто отправляем сигнал рыскания с контроллера. Им мы будем управлять вручную.
        float YawInput = RemCon.RotationIndicator.Y;

        //для каждого гироскопа устанавливаем текущие значения по тангажу, крену, рысканию.
        foreach (IMyGyro gyro in gyroList)
        {
            gyro.GyroOverride = true;
            gyro.Yaw = YawInput;
            gyro.Roll = RollInput;
            gyro.Pitch = PitchInput;
        }
    }
