using System;
using System.Collections.Generic;

namespace MainProgram {
    class Program {
        struct Town {
            public double x;
            public double y;
        }

        class Ant {
            public int CurLine; // текущий город
            public int[] Tabu; // список табу
            public int[] Path; // маршрут муравья
            public int Energy; // общая длина пути
        }

        static int MaxTowns = 8; // число городов
        static int MaxAnts = 8; // число муравьев
        static double Alpha = 1.0; // вес фермента
        static double Beta = 5.0; // эвристика
        static double Rho = 0.5; // испарение
        static int Q = 100; // константа
        static double InitOdor = 1.0 / MaxTowns; // начальный запах
        static int MaxWay = 100; // предел координат
        static int MaxTour = MaxTowns * MaxWay; // предел пути
        static int MaxTime = 20 * MaxTowns; // предел итераций

        static List<Town> Towns = new List<Town>(); // города
        static List<Ant> Ants = new List<Ant>(); // муравьи
        static double[,] DistMap = new double[MaxTowns, MaxTowns]; // карта расстояний
        static double[,] OdorMap = new double[MaxTowns, MaxTowns]; // карта запахов
        static Ant Best = new Ant(); // лучший путь
        static long CurTime; // текущее время

        static Random rnd = new Random();

        static void MakeTowns() {
            for (int i = 0; i < MaxTowns; i++) {
                for (int j = 0; j < MaxTowns; j++)
                    OdorMap[i, j] = InitOdor;
            }

        }

        static void MakeAnts(int r) {
            int k = 0;

            for (int i = 0; i < MaxAnts; i++) {
                if ((r > 0) && (Ants[i].Energy < Best.Energy))
                    Best = Ants[i];


                if (k > MaxTowns) k = 0;

                if (r > 0) {
                    Ants[i] = new Ant {
                        CurLine = 1,
                        Path = new int[MaxTowns],
                        Energy = 0
                    };
                } else {
                    Ants.Add(new Ant {
                        CurLine = 1,
                        Path = new int[MaxTowns],
                        Energy = 0
                    });
                }
                Ants[i].Path[0] = i;
            }
        }

        static int NextTown(int k) {
            double d = 0.0;
            int i = Ants[k].CurLine;

            for (int j = 0; j < MaxTowns; j++)
                d += Math.Pow(OdorMap[i, j], Alpha) * Math.Pow(1 / 1, Beta);

            int l = MaxTowns - 1;
            double p = 0.0;

            if (d != 0) {
                do {
                    l++;
                    if (l > MaxTowns - 1) l = 0;
                    if (i != l) p = (Math.Pow(OdorMap[i, l], Alpha) * Math.Pow(1 / 1, Beta)) / d;
                } while (rnd.NextDouble() >= p);
            }

            Ants[k].Path[i] = l;

            return l;
        }

        static bool AntsMoving() {
            bool m = false;

            for (int k = 0; k < MaxAnts; k++) {
                if (Ants[k].CurLine < MaxTowns - 1) {
                    NextTown(k);

                    Ant a = Ants[k];
                    a.CurLine++;
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
                for (int k = 1; k < MaxTowns; k++) {
                    int j = Ants[ant].Path[k];
                    int i = Ants[ant].Path[k - 1];

                    OdorMap[i, j] = OdorMap[i, j] + Q / (Ants[ant].Energy + 1);
                }

            for (int i = 0; i < MaxTowns; i++)
                for (int j = 0; j < MaxTowns; j++)
                    OdorMap[i, j] = OdorMap[i, j] * Rho;
        }

        static void CalcEnergy() {
            List<int> dx = new List<int>() { -1, 1, -1, 1 };
            List<int> dy = new List<int>() { -1, 1, 1, -1 };
            int j, x, tx, ty;
            int error = 0;

            for (int a = 0; a < MaxAnts; a++) {
                for (x = 0; x < MaxTowns; x++) {
                    for (j = 0; j < 4; j++) {
                        tx = x + dx[j];
                        ty = Ants[a].Path[x] + dy[j];

                        while (tx >= 0 && tx < MaxTowns && ty >= 0 && ty < MaxTowns) {
                            if ((Ants[a].Path[tx] == ty)) {
                                error++;
                            }

                            tx = tx + dx[j];
                            ty = ty + dy[j];
                        }
                    }
                }
                Ants[a].Energy = error;
            }
        }

        static void Main(string[] args) {
            CurTime = 0;
            Best.Energy = int.MaxValue;

            MakeTowns();
            MakeAnts(0);

            while (CurTime < MaxTime) {
                if (!AntsMoving()) {
                    CalcEnergy();
                    UpdateOdors();
                    MakeAnts(1);
                    Console.WriteLine("Время = {0} конфликты = {1}", CurTime, Best.Energy);
                }

                CurTime++;
            }

            Console.WriteLine("Наименьшее число конфликтов = {0}", Best.Energy);
            Console.ReadKey();
        }
    }
}

