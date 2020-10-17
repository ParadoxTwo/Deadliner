using System;
using SplashKitSDK;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
public class Program
{
    public static void Main()
    {
        Window screen = new Window("Deadliner", 800, 600);
        Course course = new Course(screen);
        course.Load();
        screen.Clear(Color.Gray);
        if(!course.HasExecuted)
            course.GetInfoFromUser();
        
        foreach(Unit unit in course._units)
            if(!unit.HasExecuted)
                unit.GetInfoFromUser();
        Dashboard dashboard = new Dashboard(screen);
        foreach(Unit unit in course._units){
            foreach (Assignment assignment in unit._assignments){
                dashboard.Add(unit, assignment);
            }
        }
        dashboard.Sort();
        dashboard.Show();
        screen.Refresh(60);
        course.Save();
        SplashKit.Delay(2000);
    }
}

[Serializable]
public class Course{
    public Window _screen;
    public string _name = "";
    public List<Unit> _units;
    public List<Unit> _remove;
    public int X {get;set;}
    public int Y {get;set;}
    public int Width {get;set;}
    public int Height {get;set;}
    public Font _font;
    public int _inp;
    public int _noOfUnits = 0;
    public bool _hasExecuted = false;
    public bool UnitsSet {get;set;}
    public BinaryFormatter formatter = new BinaryFormatter();
    public bool HasExecuted { 
        get {
            return _hasExecuted;
        }
    }
    public int  Units {get{return _noOfUnits;}}
    
    public Course(Window screen){
        _units = new List<Unit>();
        _screen = screen;
        X = screen.Width/2-300;
        Y = screen.Height/2-100;
        Width = 600;
        Height = 200;
        _font= SplashKit.LoadFont("Fira","fonts\\FiraSans-ExtraBold.otf");
        UnitsSet = false;
    }
    [Serializable]
    public struct CourseData{
        public string name;
        public int noOfUnits;
        public List<Unit.UnitData> units;
        public bool hasExecuted;
        public bool unitsSet;
    }
    
    public void Save(){
        //try{
            FileStream writer = new FileStream("course.dat", FileMode.Create, FileAccess.Write);
            CourseData c = new CourseData();
            c.name = _name;
            c.noOfUnits = _noOfUnits;
            c.unitsSet = UnitsSet;
            c.units = Unit.GetSerializedData(_units);
            c.hasExecuted = _hasExecuted;
            formatter.Serialize(writer,c);
            writer.Close();
        // }
        // catch (Exception e){
        //     Console.WriteLine("No salvation!"+e.StackTrace);
        // }
    }
    public void Load(){
        if (File.Exists("course.dat"))
        {
 
            try
            {
                // Create a FileStream will gain read access to the 
                // data file.
                FileStream readerFileStream = new FileStream("course.dat", 
                    FileMode.Open, FileAccess.Read);
                // Reconstruct information of our friends from file.
                CourseData current = (CourseData) this.formatter.Deserialize(readerFileStream);
                this._name = current.name;
                this._hasExecuted = current.hasExecuted;
                this._noOfUnits = current.noOfUnits;
                this._units = Unit.GetDeserializedData(current.units, _screen);
                // Close the readerFileStream when we are done
                readerFileStream.Close();

 
            } 
            catch (Exception e)
            {
                Console.WriteLine("Failed to load.. "+e.StackTrace);
            } // end try-catch
 
        }
    }
    public void AddUnit(Unit unit){
        _units.Add(unit);
    }
    public void RemoveUnit(string name){
        foreach(Unit unit in _units){
            if(name == unit._name){
                _remove.Add(unit);
            }
        }
        foreach(Unit unit in _remove){
            _units.Remove(unit);
        }
    }
    public void InputDraw(){
        SplashKit.DrawRectangle(Color.Black, X, Y, Width, Height);
        SplashKit.FillRectangle(Color.LightGray, X+1, Y+1, Width-2, Height-2);
        SplashKit.DrawTextOnWindow(_screen, "Name: ", Color.Black, "Fira", 20, X+10, Y+10);
        SplashKit.FillRectangle(Color.White, X+130, Y+10, 400, 20);
        SplashKit.DrawTextOnWindow(_screen, "Units: ", Color.Black, "Fira", 20, X+10, Y+50);
        SplashKit.FillRectangle(Color.White, X+130, Y+50, 400, 20);
        SplashKit.FillRectangle(Color.DarkGray, X+110, Y+150, 80, 25);
        SplashKit.DrawTextOnWindow(_screen, "Next", Color.White, "Fira", 20, X+130, Y+150);
        if(_inp==0){
            SplashKit.DrawTextOnWindow(_screen, _name, Color.Black, "Fira", 19, X+135, Y+11);
            SplashKit.DrawTextOnWindow(_screen, Convert.ToString(_noOfUnits), Color.Black, "Fira", 19, X+135, Y+51);
        }
        else if(_inp==1){
            SplashKit.DrawTextOnWindow(_screen, Convert.ToString(_noOfUnits), Color.Black, "Fira", 19, X+135, Y+51);
        }
        else if (_inp==2){
            SplashKit.DrawTextOnWindow(_screen, _name, Color.Black, "Fira", 19, X+135, Y+11);
        }
    }
    public void GetInfoFromUser(){
        while(!HasExecuted){
            //Console.WriteLine("in loop");
            InputDraw();
            SplashKit.ProcessEvents();
            HandleInput();
            _screen.Refresh(60);
        }
        if(!UnitsSet){
            for(int i=0;i<Units;i++){
                Console.WriteLine("i = "+Convert.ToString(i)+" getting info...");
                Unit unit = new Unit(_screen);
                unit.GetInfoFromUser();
                Console.WriteLine($"Unit {i+1}, Name: {unit._name}, Assignments: {unit._noOfAssignments}");
                AddUnit(unit);
            }
            UnitsSet = true;
        }
        
    }
    public void HandleInput(){
        if(SplashKit.MouseClicked(MouseButton.LeftButton)){
            Console.WriteLine(SplashKit.MousePosition().X);
            Console.WriteLine(SplashKit.MousePosition().Y);
            if(SplashKit.MousePosition().X>X+130&&SplashKit.MousePosition().X<X+530&&SplashKit.MousePosition().Y>Y+10&&SplashKit.MousePosition().Y<Y+30){
                
                switch(_inp){
                    case 1:{
                        _name = SplashKit.TextInput();
                        break;
                    }
                    case 2:{
                        _noOfUnits = Convert.ToInt32(SplashKit.TextInput());
                        break;
                    }
                }
                SplashKit.EndReadingText();
                _inp = 1;
                SplashKit.StartReadingText(new Rectangle(){X = X+130, Y = Y+10, Width = 400, Height = 20});
                //SplashKit.DrawCollectedText(Color.Black,"Fira",20);
            }
            else if(SplashKit.MousePosition().X>X+130&&SplashKit.MousePosition().X<X+530&&SplashKit.MousePosition().Y>Y+50&&SplashKit.MousePosition().Y<Y+70){
                   
                switch(_inp){
                    case 1:{
                        _name = SplashKit.TextInput();
                        break;
                    }
                    case 2:{
                        _noOfUnits = Convert.ToInt32(SplashKit.TextInput());
                        break;
                    }
                }
                    _inp = 2;
                SplashKit.EndReadingText();
                SplashKit.StartReadingText(new Rectangle(){X = X+130, Y = Y+50, Width = 400, Height = 20});
            }
            else if(SplashKit.MousePosition().X>X+110&&SplashKit.MousePosition().X<X+190&&SplashKit.MousePosition().Y>Y+150&&SplashKit.MousePosition().Y<Y+175){
            
                switch(_inp){
                    case 1: {
                        _name = SplashKit.TextInput();
                        break;
                    }
                    case 2:{
                        _noOfUnits = Convert.ToInt32(SplashKit.TextInput());
                        break;
                    }
                }
                _inp = 0;
                SplashKit.EndReadingText();
                Console.WriteLine("Done!");
                _hasExecuted = true;
            }
            else{
                Console.WriteLine(SplashKit.TextInput());
                switch(_inp){
                    case 1:{
                        _name = SplashKit.TextInput();
                        break;
                    }
                    case 2:{
                        _noOfUnits = Convert.ToInt32(SplashKit.TextInput());
                        break;
                    }
                }
                _inp = 0;
                SplashKit.EndReadingText();
            }
        }
        if(SplashKit.ReadingText()){
            //Console.WriteLine(SplashKit.TextInput());
            switch(_inp){
                case 1: {
                    SplashKit.DrawTextOnWindow(_screen, SplashKit.TextInput(), Color.Black, "Fira", 19, X+135, Y+11);
                    break;
                }
                case 2:{
                    SplashKit.DrawTextOnWindow(_screen, SplashKit.TextInput(), Color.Black, "Fira", 19, X+135, Y+51);
                    break;
                }
            }
        }
        if(SplashKit.KeyTyped(KeyCode.ReturnKey)){
            Console.WriteLine(SplashKit.TextInput());
            switch(_inp){
                case 1: {
                    _name = SplashKit.TextInput();
                    break;
                }
                case 2:{
                    _noOfUnits = Convert.ToInt32(SplashKit.TextInput());
                    break;
                }
            }
            SplashKit.EndReadingText();
            _inp = 0;
        }
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return base.ToString();
    }
}
[Serializable]
public class Assignment{
    public Window _screen;
    public string _name = "";
    public DateTime _dueDate;

    public int X {get;set;}
    public int Y {get;set;}
    public int Width {get;set;}
    public int Height {get;set;}
    public Font _font;
    public int _inp;
    public int _weightage = 0;
    public bool _hasExecuted = false;
    public bool HasExecuted { 
        get {
            return _hasExecuted;
        }
    }
    public int  Assignments {
        get {
            return _weightage;
        }
    }
    
    public Assignment(Window screen){
        _screen = screen;
        X = screen.Width/2-300;
        Y = screen.Height/2-100;
        Width = 600;
        Height = 200;
        _font= SplashKit.LoadFont("Fira","fonts\\FiraSans-ExtraBold.otf");
    }
    [Serializable]
    public struct AssignmentData {
        public string name;
        public int weightage;
        public bool hasExecuted;
        public string dueDate;
    }
    public static List<AssignmentData> GetSerializedData(List<Assignment> assignments){
        List<AssignmentData> current = new List<AssignmentData>();
        foreach(Assignment assignment in assignments){
            AssignmentData data = new AssignmentData();
            data.name = assignment._name;
            data.weightage = assignment._weightage;
            data.hasExecuted = assignment.HasExecuted;
            data.dueDate = Convert.ToString(assignment._dueDate);
            current.Add(data);
        }
        return current;
    }
    public static List<Assignment> GetDeserializedData(List<AssignmentData> assignments, Window screen){
        List<Assignment> current = new List<Assignment>();
        foreach(AssignmentData assignment in assignments){
            Assignment data = new Assignment(screen);
            data._name = assignment.name;
            data._weightage = assignment.weightage;
            data._hasExecuted = assignment.hasExecuted;
            data._dueDate = Convert.ToDateTime(assignment.dueDate);
            current.Add(data);
        }
        return current;
    }
    public void InputDraw(){
        _screen.Clear(Color.Gray);
        SplashKit.DrawRectangle(Color.Black, X, Y, Width, Height);
        SplashKit.FillRectangle(Color.LightGray, X+1, Y+1, Width-2, Height-2);
        SplashKit.DrawTextOnWindow(_screen, "Name: ", Color.Black, "Fira", 20, X+10, Y+10);
        SplashKit.FillRectangle(Color.White, X+130, Y+10, 400, 20);
        SplashKit.DrawTextOnWindow(_screen, "Weightage: ", Color.Black, "Fira", 20, X+10, Y+50);
        SplashKit.FillRectangle(Color.White, X+130, Y+50, 400, 20);
        SplashKit.DrawTextOnWindow(_screen, "Due Date: ", Color.Black, "Fira", 20, X+10, Y+90);
        SplashKit.FillRectangle(Color.White, X+130, Y+90, 400, 20);
        SplashKit.FillRectangle(Color.DarkGray, X+110, Y+150, 80, 25);
        SplashKit.DrawTextOnWindow(_screen, "Add", Color.White, "Fira", 20, X+130, Y+150);
        if(_inp==0){
            SplashKit.DrawTextOnWindow(_screen, _name, Color.Black, "Fira", 19, X+135, Y+11);
            SplashKit.DrawTextOnWindow(_screen, Convert.ToString(_weightage), Color.Black, "Fira", 19, X+135, Y+51);
            SplashKit.DrawTextOnWindow(_screen, Convert.ToString(_dueDate), Color.Black, "Fira", 19, X+135, Y+91);
        }
        else if(_inp==1){
            SplashKit.DrawTextOnWindow(_screen, Convert.ToString(_weightage), Color.Black, "Fira", 19, X+135, Y+51);
            SplashKit.DrawTextOnWindow(_screen, Convert.ToString(_dueDate), Color.Black, "Fira", 19, X+135, Y+91);
        }
        else if (_inp==2){
            SplashKit.DrawTextOnWindow(_screen, Convert.ToString(_dueDate), Color.Black, "Fira", 19, X+135, Y+91);
            SplashKit.DrawTextOnWindow(_screen, _name, Color.Black, "Fira", 19, X+135, Y+11);
        }
        else if (_inp==3){
            SplashKit.DrawTextOnWindow(_screen, _name, Color.Black, "Fira", 19, X+135, Y+11);
            SplashKit.DrawTextOnWindow(_screen, Convert.ToString(_weightage), Color.Black, "Fira", 19, X+135, Y+51);
        }
    }
    public void GetInfoFromUser(){
        while(!HasExecuted){
            //Console.WriteLine("in loop");
            InputDraw();
            SplashKit.ProcessEvents();
            HandleInput();
            _screen.Refresh(60);
        }

    }
    public void HandleInput(){
        if(SplashKit.MouseClicked(MouseButton.LeftButton)){
            Console.WriteLine(SplashKit.MousePosition().X);
            Console.WriteLine(SplashKit.MousePosition().Y);
            if(SplashKit.MousePosition().X>X+130&&SplashKit.MousePosition().X<X+530&&SplashKit.MousePosition().Y>Y+10&&SplashKit.MousePosition().Y<Y+30){
                
                switch(_inp){
                    case 1:{
                        _name = SplashKit.TextInput();
                        break;
                    }
                    case 2:{
                        _weightage = Convert.ToInt32(SplashKit.TextInput());
                        break;
                    }
                    case 3:{
                        _dueDate = Convert.ToDateTime(SplashKit.TextInput());
                        break;
                    }
                }
                SplashKit.EndReadingText();
                _inp = 1;
                SplashKit.StartReadingText(new Rectangle(){X = X+130, Y = Y+10, Width = 400, Height = 20});
                //SplashKit.DrawCollectedText(Color.Black,"Fira",20);
            }
            else if(SplashKit.MousePosition().X>X+130&&SplashKit.MousePosition().X<X+530&&SplashKit.MousePosition().Y>Y+50&&SplashKit.MousePosition().Y<Y+70){
                   
                switch(_inp){
                    case 1:{
                        _name = SplashKit.TextInput();
                        break;
                    }
                    case 2:{
                        _weightage = Convert.ToInt32(SplashKit.TextInput());
                        break;
                    }
                    case 3:{
                        _dueDate = Convert.ToDateTime(SplashKit.TextInput());
                        break;
                    }
                }
                    _inp = 2;
                SplashKit.EndReadingText();
                SplashKit.StartReadingText(new Rectangle(){X = X+130, Y = Y+50, Width = 400, Height = 20});
            }
            else if(SplashKit.MousePosition().X>X+130&&SplashKit.MousePosition().X<X+530&&SplashKit.MousePosition().Y>Y+90&&SplashKit.MousePosition().Y<Y+110){
                   
                switch(_inp){
                    case 1:{
                        _name = SplashKit.TextInput();
                        break;
                    }
                    case 2:{
                        _weightage = Convert.ToInt32(SplashKit.TextInput());
                        break;
                    }
                    case 3:{
                        _dueDate = Convert.ToDateTime(SplashKit.TextInput());
                        break;
                    }
                }
                    _inp = 3;
                SplashKit.EndReadingText();
                SplashKit.StartReadingText(new Rectangle(){X = X+130, Y = Y+90, Width = 400, Height = 20});
            }
            else if(SplashKit.MousePosition().X>X+110&&SplashKit.MousePosition().X<X+190&&SplashKit.MousePosition().Y>Y+150&&SplashKit.MousePosition().Y<Y+175){
            
                switch(_inp){
                    case 1: {
                        _name = SplashKit.TextInput();
                        break;
                    }
                    case 2:{
                        _weightage = Convert.ToInt32(SplashKit.TextInput());
                        break;
                    }
                    case 3:{
                        _dueDate = Convert.ToDateTime(SplashKit.TextInput());
                        break;
                    }
                }
                _inp = 0;
                SplashKit.EndReadingText();
                Console.WriteLine("Done!");
                _hasExecuted = true;
            }
            else{
                Console.WriteLine(SplashKit.TextInput());
                switch(_inp){
                    case 1:{
                        _name = SplashKit.TextInput();
                        break;
                    }
                    case 2:{
                        _weightage = Convert.ToInt32(SplashKit.TextInput());
                        break;
                    }
                    case 3:{
                        _dueDate = Convert.ToDateTime(SplashKit.TextInput());
                        break;
                    }
                }
                _inp = 0;
                SplashKit.EndReadingText();
            }
        }
        if(SplashKit.ReadingText()){
            //Console.WriteLine(SplashKit.TextInput());
            switch(_inp){
                case 1: {
                    SplashKit.DrawTextOnWindow(_screen, SplashKit.TextInput(), Color.Black, "Fira", 19, X+135, Y+11);
                    break;
                }
                case 2:{
                    SplashKit.DrawTextOnWindow(_screen, SplashKit.TextInput(), Color.Black, "Fira", 19, X+135, Y+51);
                    break;
                }
                case 3:{
                    SplashKit.DrawTextOnWindow(_screen, SplashKit.TextInput(), Color.Black, "Fira", 19, X+135, Y+91);
                    break;
                }
            }
        }
        if(SplashKit.KeyTyped(KeyCode.ReturnKey)){
            Console.WriteLine(SplashKit.TextInput());
            switch(_inp){
                case 1: {
                    _name = SplashKit.TextInput();
                    break;
                }
                case 2:{
                    _weightage = Convert.ToInt32(SplashKit.TextInput());
                    break;
                }
                case 3:{
                    _dueDate = Convert.ToDateTime(SplashKit.TextInput());
                    break;
                }
            }
            SplashKit.EndReadingText();
            _inp = 0;
        }
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return base.ToString();
    }
}
public class Dashboard{
    public struct DashboardData {
        public string unitName;
        public string assignmentName;
        public DateTime dueDate;
        public int weightage;
    }
    List<DashboardData> _collection = new List<DashboardData>();
    Window _screen;
    public int X {get;set;}
    public int Y {get;set;}
    public int Width {get;set;}
    public int Height {get;set;}
    public Font _font;
    public Dashboard(Window screen){
        _screen = screen;
        X = screen.Width/2-300;
        Y = screen.Height/2-240;
        Width = 550;
        Height = 460;
        _font= SplashKit.LoadFont("Fira","fonts\\FiraSans-ExtraBold.otf");
    }
    public void Add(Unit unit, Assignment assignment){
        _collection.Add(new DashboardData(){
            unitName=unit._name,
            assignmentName=assignment._name,
            dueDate=assignment._dueDate,
            weightage=assignment._weightage
        });

    }
    public void Sort(){
        _collection.Sort((x,y)=> x.dueDate.CompareTo(y.dueDate));
        Console.WriteLine("Initial sorting:");
        foreach(DashboardData data in _collection){
            Console.WriteLine($"Due Date: {data.dueDate.ToString()}, Weightage: {data.weightage}");
        }
        List<List<DashboardData>> l = new List<List<DashboardData>>();
        int i=0,j=0;
        foreach(DashboardData data in _collection){
            if(l.Count==0)
                l.Add(new List<DashboardData>());
            if(l[i].Count==0)
                l[i].Add(data);
            else if(l[i][j].dueDate==data.dueDate){
                l[i].Add(data);
                j++;
            }
            else {//if(l[i][j].dueDate!=data.dueDate){
                 if(l[i][j].dueDate==null){
                     l.Add(new List<DashboardData>());
                     l[i].Add(data);
                 }
                 else{
                     l.Add(new List<DashboardData>());
                     i++;
                     l[i].Add(data);
                     j=0;
                 }
            }
        }
        List<DashboardData> final = new List<DashboardData>();
        // foreach(List<DashboardData> data in l)
        //     data.Sort((x,y)=>x.weightage.CompareTo(y.weightage));
        foreach(List<DashboardData> d1 in l){
            d1.Sort((x,y)=>y.weightage.CompareTo(x.weightage));
            foreach(DashboardData d2 in d1)
                final.Add(d2);
        }
        Console.WriteLine("Final sorting:");
        foreach(DashboardData data in final){
            Console.WriteLine($"Due Date: {data.dueDate.ToString()}, Weightage: {data.weightage}");
        }
    }
    public Color GetUgrencyColor(DashboardData data){
        Color color;
        DateTime today = DateTime.Today;
        TimeSpan seven = new TimeSpan(7,0,0,0);
        TimeSpan one = new TimeSpan(1,0,0,0);
        TimeSpan zero = new TimeSpan(0,0,0,0);
        if(data.dueDate.Subtract(today)>seven){
            color = Color.LightBlue;
        }
        else if(data.dueDate.Subtract(today)>one){
            color = Color.LightGreen;
        }
        else if(data.dueDate.Subtract(today)>zero){
            color = Color.Orange;
        }
        else{
            color = Color.Red;
        }
        return color;
    }
    public void Show(){
        const int WIDTH = 200, HEIGHT = 100, GAP = 30;
        SplashKit.ProcessEvents();
        while(!SplashKit.KeyTyped(KeyCode.EscapeKey)){
            int i = 0, j = 0;
            SplashKit.ProcessEvents();
            _screen.Clear(Color.Gray);
            SplashKit.FillRectangle(Color.LightGray, X, Y, Width, Height);
            foreach(DashboardData assignment in _collection){
                int x = X+WIDTH*j+GAP, y = Y+HEIGHT*i+GAP;
                SplashKit.DrawRectangle(Color.Black, x, y, WIDTH, HEIGHT);
                SplashKit.FillRectangle(GetUgrencyColor(assignment), x+1, y+1, WIDTH-2, HEIGHT-2);
                SplashKit.DrawTextOnWindow(_screen,assignment.unitName,Color.Black,"Fira", 25, x+5, y+5);
                SplashKit.DrawTextOnWindow(_screen,assignment.assignmentName,Color.Black,"Fira", 20, x+5, y+30);
                SplashKit.DrawTextOnWindow(_screen,assignment.dueDate.Day.ToString()+"-"+assignment.dueDate.Month.ToString()+"-"+assignment.dueDate.Year.ToString(),Color.Black,"Fira", 20, x+5, y+50);
                SplashKit.DrawTextOnWindow(_screen,assignment.dueDate.Subtract(DateTime.Today).Days.ToString()+" days left",Color.Black,"Fira", 20, x+5, y+75);
                
                if(++i>3){
                    j++;
                    i=0;
                }
            }
            _screen.Refresh(60);
        }
    }
}

[Serializable]
public class Unit{
    public Window _screen;
    public string _name = "";
    public List<Assignment> _assignments;
    public List<Assignment> _remove;
    public int X {get;set;}
    public int Y {get;set;}
    public int Width {get;set;}
    public int Height {get;set;}
    public Font _font;
    public int _inp;
    public int _noOfAssignments = 0;
    public bool _hasExecuted = false;
    public bool AssingmentsSet {get;set;}
    public bool HasExecuted { 
        get {
            return _hasExecuted;
        }
    }
    public int  Assignments {
        get {
            return _noOfAssignments;
        }
    }
    
    public Unit(Window screen){
        _assignments = new List<Assignment>();
        _screen = screen;
        X = screen.Width/2-300;
        Y = screen.Height/2-100;
        Width = 600;
        Height = 200;
        _font= SplashKit.LoadFont("Fira","fonts\\FiraSans-ExtraBold.otf");
    }
    public void AddAssignment(Assignment assignment){
        _assignments.Add(assignment);
    }
    public void RemoveAssignment(string name){
        foreach(Assignment assignment in _assignments){
            if(name == assignment._name){
                _remove.Add(assignment);
            }
        }
        foreach(Assignment assignment in _remove){
            _assignments.Remove(assignment);
        }
    }
    [Serializable]
    public struct UnitData {
        public string name;
        public int noOfAssignments;
        public List<Assignment.AssignmentData> assignments;
        public bool hasExecuted;
        public bool assingmentsSet;
    }
    public static List<UnitData> GetSerializedData(List<Unit> units){
        List<UnitData> current = new List<UnitData>();
        foreach(Unit unit in units){
            UnitData data = new UnitData();
            data.name = unit._name;
            data.noOfAssignments = unit._noOfAssignments;
            data.hasExecuted = unit.HasExecuted;
            data.assingmentsSet = unit.AssingmentsSet;
            data.assignments = Assignment.GetSerializedData(unit._assignments);
            current.Add(data);
        }
        return current;
    }
    public static List<Unit> GetDeserializedData(List<UnitData> units, Window screen){
        List<Unit> current = new List<Unit>();
        foreach(UnitData unit in units){
            Unit data = new Unit(screen);
            data._assignments = Assignment.GetDeserializedData(unit.assignments, screen);
            data._name = unit.name;
            data._noOfAssignments = unit.noOfAssignments;
            data._hasExecuted = unit.hasExecuted;
            data.AssingmentsSet = unit.assingmentsSet;
            current.Add(data);
        }
        return current;
    }
    public void InputDraw(){
        _screen.Clear(Color.Gray);
        SplashKit.DrawRectangle(Color.Black, X, Y, Width, Height);
        SplashKit.FillRectangle(Color.LightGray, X+1, Y+1, Width-2, Height-2);
        SplashKit.DrawTextOnWindow(_screen, "Name: ", Color.Black, "Fira", 20, X+10, Y+10);
        SplashKit.FillRectangle(Color.White, X+130, Y+10, 400, 20);
        SplashKit.DrawTextOnWindow(_screen, "Assignments: ", Color.Black, "Fira", 20, X+10, Y+50);
        SplashKit.FillRectangle(Color.White, X+130, Y+50, 400, 20);
        SplashKit.FillRectangle(Color.DarkGray, X+110, Y+150, 80, 25);
        SplashKit.DrawTextOnWindow(_screen, "Next", Color.White, "Fira", 20, X+130, Y+150);
        if(_inp==0){
            SplashKit.DrawTextOnWindow(_screen, _name, Color.Black, "Fira", 19, X+135, Y+11);
            SplashKit.DrawTextOnWindow(_screen, Convert.ToString(_noOfAssignments), Color.Black, "Fira", 19, X+135, Y+51);
        }
        else if(_inp==1){
            SplashKit.DrawTextOnWindow(_screen, Convert.ToString(_noOfAssignments), Color.Black, "Fira", 19, X+135, Y+51);
        }
        else if (_inp==2){
            SplashKit.DrawTextOnWindow(_screen, _name, Color.Black, "Fira", 19, X+135, Y+11);
        }
    }
    public void GetInfoFromUser(){
        while(!HasExecuted){
            //Console.WriteLine("in loop");
            InputDraw();
            SplashKit.ProcessEvents();
            HandleInput();
            _screen.Refresh(60);
        }
        if(!AssingmentsSet){
            for(int i=0;i<Assignments;i++){
                Assignment assignment = new Assignment(_screen);
                assignment.GetInfoFromUser();
                AddAssignment(assignment);
            }
            AssingmentsSet = true;
        }
        
    }
    public void HandleInput(){
        if(SplashKit.MouseClicked(MouseButton.LeftButton)){
            Console.WriteLine(SplashKit.MousePosition().X);
            Console.WriteLine(SplashKit.MousePosition().Y);
            if(SplashKit.MousePosition().X>X+130&&SplashKit.MousePosition().X<X+530&&SplashKit.MousePosition().Y>Y+10&&SplashKit.MousePosition().Y<Y+30){
                
                switch(_inp){
                    case 1:{
                        _name = SplashKit.TextInput();
                        break;
                    }
                    case 2:{
                        _noOfAssignments = Convert.ToInt32(SplashKit.TextInput());
                        break;
                    }
                }
                SplashKit.EndReadingText();
                _inp = 1;
                SplashKit.StartReadingText(new Rectangle(){X = X+130, Y = Y+10, Width = 400, Height = 20});
                //SplashKit.DrawCollectedText(Color.Black,"Fira",20);
            }
            else if(SplashKit.MousePosition().X>X+130&&SplashKit.MousePosition().X<X+530&&SplashKit.MousePosition().Y>Y+50&&SplashKit.MousePosition().Y<Y+70){
                   
                switch(_inp){
                    case 1:{
                        _name = SplashKit.TextInput();
                        break;
                    }
                    case 2:{
                        _noOfAssignments = Convert.ToInt32(SplashKit.TextInput());
                        break;
                    }
                }
                    _inp = 2;
                SplashKit.EndReadingText();
                SplashKit.StartReadingText(new Rectangle(){X = X+130, Y = Y+50, Width = 400, Height = 20});
            }
            else if(SplashKit.MousePosition().X>X+110&&SplashKit.MousePosition().X<X+190&&SplashKit.MousePosition().Y>Y+150&&SplashKit.MousePosition().Y<Y+175){
            
                switch(_inp){
                    case 1: {
                        _name = SplashKit.TextInput();
                        break;
                    }
                    case 2:{
                        _noOfAssignments = Convert.ToInt32(SplashKit.TextInput());
                        break;
                    }
                }
                _inp = 0;
                SplashKit.EndReadingText();
                Console.WriteLine("Done!");
                _hasExecuted = true;
            }
            else{
                Console.WriteLine(SplashKit.TextInput());
                switch(_inp){
                    case 1:{
                        _name = SplashKit.TextInput();
                        break;
                    }
                    case 2:{
                        _noOfAssignments = Convert.ToInt32(SplashKit.TextInput());
                        break;
                    }
                }
                _inp = 0;
                SplashKit.EndReadingText();
            }
        }
        if(SplashKit.ReadingText()){
            //Console.WriteLine(SplashKit.TextInput());
            switch(_inp){
                case 1: {
                    SplashKit.DrawTextOnWindow(_screen, SplashKit.TextInput(), Color.Black, "Fira", 19, X+135, Y+11);
                    break;
                }
                case 2:{
                    SplashKit.DrawTextOnWindow(_screen, SplashKit.TextInput(), Color.Black, "Fira", 19, X+135, Y+51);
                    break;
                }
            }
        }
        if(SplashKit.KeyTyped(KeyCode.ReturnKey)){
            Console.WriteLine(SplashKit.TextInput());
            switch(_inp){
                case 1: {
                    _name = SplashKit.TextInput();
                    break;
                }
                case 2:{
                    _noOfAssignments = Convert.ToInt32(SplashKit.TextInput());
                    break;
                }
            }
            SplashKit.EndReadingText();
            _inp = 0;
        }
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return base.ToString();
    }
}
