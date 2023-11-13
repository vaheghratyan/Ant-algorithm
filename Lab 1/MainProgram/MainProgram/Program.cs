using System;
using System.Collections.Generic;

namespace MainProgram {
    class Program {
        struct Town {
            public double x;
            public double y;
        }

        struct Ant {
            public int CurTown; // текущий город
            public int[] Tabu; // табу
            public int[] Path; // маршрут муравья
            public int TownsNum; // число городов в маршруте
            public double Length; // общая длина пути
        }

        static int MaxTowns = 30; //число городов
        static int MaxAnts = 30; //число муравьев
        static double Alpha = 1.0; //вес фермента
        static double Beta = 5.0; //эвристика
        static double Rho = 0.5; //испарение
        static int Q = 100; //константа
        static double InitOdor = 1.0 / MaxTowns; //начальный запах
        static int MaxWay = 100; //предел координат
        static int MaxTour = MaxTowns * MaxWay; //предел пути
        static int MaxTime = 20 * MaxTowns; //предел итераций

        static List<Town> Towns = new List<Town>(); //города
        static List<Ant> Ants = new List<Ant>(); //муравьи
        static double[,] DistMap = new double[MaxTowns, MaxTowns]; //карта расстояний
        static double[,] OdorMap = new double[MaxTowns, MaxTowns]; //карта запахов
        static Ant Best = new Ant(); //лучший путь
        static long CurTime; //текущее время

        static Random rnd = new Random();

        static void MakeTowns() {
            for (int i = 0; i < MaxTowns; i++) {
                Towns.Add(new Town {
                    x = rnd.NextDouble() * MaxWay + 1, y = rnd.NextDouble() * MaxWay + 1
                });

                for (int j = 0; j < MaxTowns; j++)
                    OdorMap[i, j] = InitOdor;
            }

            for (int i = 0; i < MaxTowns; i++) {
                DistMap[i, i] = 0;

                for (int j = i + 1; j < MaxTowns; j++) {
                    double xd = Towns[i].x - Towns[j].x;
                    double yd = Towns[i].y - Towns[j].y;

                    DistMap[i, j] = Math.Sqrt(xd * xd + yd * yd);
                    DistMap[j, i] = DistMap[i, j];
                }
            }
        }

        static void MakeAnts(int r) {
            int k = 0;

            for (int i = 0; i < MaxAnts; i++) {
                if ((r > 0) && (Ants[i].Length < Best.Length))
                    Best = Ants[i];

                if (k > MaxTowns)
                    k = 0;

                if (r > 0) {
                    Ants[i] = new Ant {
                        CurTown = k++,
                        Tabu = new int[MaxTowns],
                        Path = new int[MaxTowns],
                        TownsNum = 1,
                        Length = 0
                    };
                } else {
                    Ants.Add(new Ant {
                        CurTown = k++,
                        Tabu = new int[MaxTowns],
                        Path = new int[MaxTowns],
                        TownsNum = 1,
                        Length = 0
                    });
                }

                Ants[i].Tabu[Ants[i].CurTown] = 1;
                Ants[i].Path[0] = Ants[i].CurTown;
            }
        }

        static int NextTown(int k) {
            double d = 0.0;
            int i = Ants[k].CurTown;

            for (int j = 0; j < MaxTowns; j++)
                if (Ants[k].Tabu[j] == 0)
                    d += Math.Pow(OdorMap[i, j], Alpha) * Math.Pow(1 / DistMap[i, j], Beta);

            int l = MaxTowns - 1;
            double p = 0.0;

            if (d != 0) {
                do {
                    l++;

                    if (l > MaxTowns - 1)
                        l = 0;
                    if (i != l)
                        p = (Math.Pow(OdorMap[i, l], Alpha) * Math.Pow(1 / DistMap[i, l], Beta)) / d;
                } while ((Ants[k].Tabu[l] != 0) || (rnd.NextDouble() >= p));
            }

            return l;
        }

        static bool AntsMoving() {
            bool m = false;
            int Next;

            for (int k = 0; k < MaxAnts; k++) {
                if (Ants[k].TownsNum < MaxTowns - 1) {
                    Next = NextTown(k);

                    Ant a = Ants[k];
                    a.TownsNum++;

                    a.Path[a.TownsNum - 1] = Next;
                    a.Tabu[Next] = 1;
                    a.Length = a.Length + DistMap[a.CurTown, Next];

                    if (a.TownsNum == MaxTowns)
                        a.Length = a.Length + DistMap[a.Path[MaxTowns - 1], a.Path[0]];
                    a.CurTown = Next;

                    Ants[k] = a;

                    m = true;
                }
            }

            return m;
        }

        static void UpdateOdors() {
            for (int i = 0; i < MaxTowns; i++)
                for (int j = 0; j < MaxTowns; j++)
                    if (i != j) {
                        OdorMap[i, j] = OdorMap[i, j] * (1 - Rho);
                        if (OdorMap[i, j] < InitOdor) OdorMap[i, j] = InitOdor;
                    }

            for (int ant = 0; ant < MaxAnts; ant++)
                for (int k = 0; k < MaxTowns; k++) {
                    int j, i = Ants[ant].Path[k];

                    if (k < MaxTowns - 1)
                        j = Ants[ant].Path[k + 1];
                    else
                        j = Ants[ant].Path[0];

                    OdorMap[i, j] = OdorMap[i, j] + Q / Ants[ant].Length;
                    OdorMap[j, i] = OdorMap[i, j];
                }

            for (int i = 0; i < MaxTowns; i++)
                for (int j = 0; j < MaxTowns; j++)
                    OdorMap[i, j] = OdorMap[i, j] * Rho;
        }

        static void Main(string[] args) {
            CurTime = 0;
            Best.Length = MaxTour;

            MakeTowns();
            MakeAnts(0);

            while (CurTime < MaxTime) {
                if (!AntsMoving()) {
                    UpdateOdors();
                    MakeAnts(1);

                    Console.WriteLine("Время = {0} Путь = {1}", CurTime, Best.Length);
                }

                CurTime++;
            }

            Console.WriteLine("Оптимальный путь = {0}", Best.Length);
            Console.ReadKey();
        }
    }
}


