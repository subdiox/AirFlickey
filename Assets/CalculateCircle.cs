using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CalculateCircle {
    public static int bunkatu = 11; // 適当な値（候補として挙げたい円の半分）
    static double min_v = 1000000; // 分散最小となる候補円、つまり意図円の分散値
    public static double center_vx = 0; // 同上のx座標
    public static double center_vy = 0; // 同上のy座標
    static double min_dist = 0; // 同上の半径
    public static double m_start_x = 0; // 書き始め（始点）x座標
    public static double m_start_y = 0; // 書き始め（終点）y座標
    static float lastAngle = 0;
    static float amountAngle = 0;
    static float lastJumpAmountAngle = 0;
    public static int skipCount = 6;
    private static int queueSize = 15;
    public static FixedSizeQueue<Vector2> m_Tracequeue;
    private static FixedSizeQueue<float> m_AngleAvequeue;
    private static Vector2 firstVec2;
    private static int lastvirtualCenter = 0;
    private static int tmpvirtualCenter = 0;
    static bool skipcalcenter = false;

    static List<Vector2> m_VirtualCenterList1;
    static List<Vector2> m_VirtualCenterList2;

    static bool iscalCenter = true;
    static bool isMoving = true;

    static List<Vector2> circleList;
    static int calCentarnum = 1;
    static int nwse = 0;
    static int count = 0;

    public static double tmpcenter_vx = 0; // 同上のx座標
    public static double tmpcenter_vy = 0; // 同上のy座標

    public static void Initialize() {
        count = 0;
        circleList = new List<Vector2>();
        m_Tracequeue = new FixedSizeQueue<Vector2>(queueSize);
        m_AngleAvequeue = new FixedSizeQueue<float>(3);
        min_v = 1000000;
        iscalCenter = true;
        lastAngle = 0;
        amountAngle = 0;
        lastJumpAmountAngle = 0;
    }

    public static CalculateCircleData GetCalculateCirclebyFixedCenterwithFixStart(float end_x, float end_y) {
        m_start_x = center_vx;
        m_start_y = 1;
        firstVec2 = new Vector2((float)center_vx,1);
        // 角度算出
        float angle = CalculateAngle(end_x, end_y);

        CalculateCircleData data = new CalculateCircleData();

        data.isCalculated = isCalculatedAngle(angle);
        data.center_x = (float)center_vx;
        data.center_y = (float)center_vy;
        data.angle = (float)angle;
        data.startAngle = GetStartAngle();
        data.addAngle = CalculateAngleDiff(angle, lastAngle);
        amountAngle = amountAngle + data.addAngle;
        data.amountAngle = amountAngle;
        setJumpData(data, angle, lastAngle);
        if (isJumped(angle, lastAngle)) {
            data.isJumped = true;
        }
        lastAngle = (float)angle;

        return data;
    }

    public static CalculateCircleData GetCalculateCirclebyFixedCenter(float end_x, float end_y) {
        if (m_Tracequeue.Count() == 0) {
            m_Tracequeue.Enqueue(new Vector2(end_x, end_y));
            m_start_x = end_x;
            m_start_y = end_y;
            firstVec2 = m_Tracequeue.Peek();
        }
        // 角度算出
        float angle = CalculateAngle(end_x, end_y);

        CalculateCircleData data = new CalculateCircleData();

        data.isCalculated = isCalculatedAngle(angle);
        data.center_x = (float)center_vx;
        data.center_y = (float)center_vy;
        data.angle = (float)angle;
        data.startAngle = GetStartAngle();
        data.addAngle = CalculateAngleDiff(angle, lastAngle);
        amountAngle = amountAngle + data.addAngle;
        data.amountAngle = amountAngle;
        setJumpData(data, angle, lastAngle);
        if (isJumped(angle, lastAngle)) {
            data.isJumped = true;
        }
        lastAngle = (float)angle;

        return data;
    }

    private static float GetStartAngle() {
        float angle = Vector2.SignedAngle(Vector2.up, firstVec2 - new Vector2((float)center_vx, (float)center_vy));

        if (angle < 0) {
            angle = -angle;
        } else {
            angle = 360 - angle;
        }

        return angle;
    }

    private static bool isCalculatedAngle(float angle) {
        return !float.IsNaN(angle);
    }

    public static CalculateCircleData GetCalculateCircleDataPerTarce(float end_x, float end_y) {
        m_VirtualCenterList1 = new List<Vector2>();
        m_VirtualCenterList2 = new List<Vector2>();

        count++;

        if (m_Tracequeue.Count() <= skipCount) {
            m_Tracequeue.Enqueue(new Vector2(end_x, end_y));
        } else if (isMoving && count % 2 == 0) {
            m_Tracequeue.Enqueue(new Vector2(end_x, end_y));
        }
        CalculateCircleData data = null;

        if (m_Tracequeue.Count() == 1) {
            m_start_x = end_x;
            m_start_y = end_y;
            firstVec2 = m_Tracequeue.Peek();
            circleList.Add(firstVec2);
        }

        //中心点算出
        if (CalculateCenter(null, m_start_x, m_start_y, end_x, end_y)) {
            //角度算出
            float angle = CalculateAngle(end_x, end_y);

            isSkipCal(angle);

            data = new CalculateCircleData();
            data.isCalculated = isCalculatedAngle(angle);            
            data.center_x = (float)center_vx;
            data.center_y = (float)center_vy;
            data.angle = (float)angle;
            data.startAngle = GetStartAngle();
            m_AngleAvequeue.Enqueue(CalculateAngleDiff(angle, lastAngle));
            data.addAngle = m_AngleAvequeue.queue.Average();

            if (Mathf.Abs(m_AngleAvequeue.queue.Sum()) <= 5) {
                isMoving = false;
            } else {
                isMoving = true;
            }

            data.amountAngle = amountAngle;
            setJumpData(data, angle, lastAngle);

            if (Mathf.Abs(data.addAngle) < 90f) { //外れ値対策
                amountAngle = amountAngle + data.addAngle;
                lastAngle = (float)angle;
            }

            skipcalcenter = true;
            lastvirtualCenter = tmpvirtualCenter;
            return data;
        } else {
            // 角度算出
            float angle = CalculateAngle(end_x, end_y);

            data = new CalculateCircleData();
            data.isCalculated = false;
            data.center_x = (float)center_vx;
            data.center_y = (float)center_vy;
            data.angle = (float)angle;
            data.virtualCenterList1.AddRange(m_VirtualCenterList1);
            data.virtualCenterList2.AddRange(m_VirtualCenterList2);
            data.amountAngle = 0;
            data.rotationNumber = 0;

            lastAngle = (float)angle;
            return data;
        }
    }

    public static CalculateCircleData GetCalculateCircleDataPertmpTarce(float end_x, float end_y) {
        m_VirtualCenterList1 = new List<Vector2>();
        m_VirtualCenterList2 = new List<Vector2>();

        count++;

        if (m_Tracequeue.Count() <= skipCount) {
            m_Tracequeue.Enqueue(new Vector2(end_x, end_y));
        } else if (isMoving && count % 2 == 0) {
            m_Tracequeue.Enqueue(new Vector2(end_x, end_y));
        }

        CalculateCircleData data = null;

        if (m_Tracequeue.Count() == 1) {
            m_start_x = end_x;
            m_start_y = end_y;
            firstVec2 = m_Tracequeue.Peek();
            circleList.Add(firstVec2);
        }

        // 中心点算出
        if (tmpCalculateCenter(null, m_start_x, m_start_y, end_x, end_y)) {
            // 角度算出
            float angle = tmpCalculateAngle(end_x, end_y);

            isSkipCal(angle);

            data = new CalculateCircleData();
            data.isCalculated = isCalculatedAngle(angle);
            data.center_x = (float)tmpcenter_vx;
            data.center_y = (float)tmpcenter_vy;
            data.angle = (float)angle;
            data.startAngle = GetStartAngle();

            m_AngleAvequeue.Enqueue(CalculateAngleDiff(angle, lastAngle));
            data.addAngle = m_AngleAvequeue.queue.Average();

            if (Mathf.Abs(m_AngleAvequeue.queue.Sum()) <= 5) {
                isMoving = false;
            } else {
                isMoving = true;
            }

            data.amountAngle = amountAngle;
            setJumpData(data, angle, lastAngle);

            if (Mathf.Abs(data.addAngle) < 90f) { //外れ値対策
                amountAngle = amountAngle + data.addAngle;
                lastAngle = (float)angle;
            }

            skipcalcenter = true;
            lastvirtualCenter = tmpvirtualCenter;
            return data;
        } else {
            // 角度算出
            float angle = tmpCalculateAngle(end_x, end_y);

            data = new CalculateCircleData();
            data.isCalculated = false;
            data.center_x = (float)tmpcenter_vx;
            data.center_y = (float)tmpcenter_vy;
            data.angle = (float)angle;
            data.virtualCenterList1.AddRange(m_VirtualCenterList1);
            data.virtualCenterList2.AddRange(m_VirtualCenterList2);
            data.amountAngle = 0;
            data.rotationNumber = 0;

            lastAngle = (float)angle;
            return data;
        }
    }

    static void isSkipCal(float angle) {
        iscalCenter = caljudge(angle);
    }

    static bool caljudge(float angle) {
        if (150f < angle && angle < 210f) {
            return false;
        }

        if (60f > angle || angle > 300) {
            return false;
        }

        if (nwse != 1 && 32f < angle && angle < 58) {
            nwse = 1;
        } else if (nwse != 2 && 77f < angle && angle < 103f) {
            nwse = 2;
            return true;
        } else if (nwse != 3 && 112f < angle && angle < 148f) {
            nwse = 31;
        } else if (nwse != 4 && 167f < angle && angle < 193f) {
            nwse = 4;
            return true;
        } else if (nwse != 5 && 202f < angle && angle < 238f) {
            nwse = 5;
        } else if (nwse != 6 && 257f < angle && angle < 283f) {
            nwse = 6;
            return true;
        } else if (nwse != 7 && 292f < angle && angle < 328) {
            nwse = 7;
        } else if (nwse != 8 && (angle < 13 || angle > 347)) {
            nwse = 8;
            return true;
        }
        return false;
    }

    private static void setJumpData(CalculateCircleData data,  float angle, float lastAngle) {
        if (isJumped(angle, lastAngle)) {
            data.isJumped = true;
            data.isJumpedClockwise = angle < lastAngle;

            if (Mathf.Abs(lastJumpAmountAngle - amountAngle) >= 180f) {
                if (data.isJumpedClockwise) {
                    data.rotationNumber = 1;
                } else {
                    data.rotationNumber = -1;
                }
                lastJumpAmountAngle = amountAngle;
            }
        } else {
            data.rotationNumber = 0;
        }
    }

    private static bool isJumped(float angle, float lastAngle) {
        return Mathf.Abs(angle - lastAngle) >= 180f;
    }

    private static float CalculateAngleDiff(float angle, float lastAngle) {
        if (Mathf.Abs(angle - lastAngle) <= 180f) {
            return angle - lastAngle;
        } else {
            if (angle < lastAngle) { // 360 → 0 のジャンプ
                return 360 - (lastAngle - angle);
            } else { // 0 → 360のジャンプ
                return (angle - lastAngle) - 360;
            }
        }
    }

    public static float CalculateAngle(double end_x, double end_y) {
        // 始点、中心点の線分
        double aa = Math.Sqrt((m_start_x - center_vx) * (m_start_x - center_vx) + (m_start_y - center_vy) * (m_start_y - center_vy));
        // 終点、中心点の線分
        double bb = Math.Sqrt((end_x - center_vx) * (end_x - center_vx) + (end_y - center_vy) * (end_y - center_vy));
        // 角度の算出
        double coss = ((m_start_x - center_vx) * (end_x - center_vx) + (m_start_y - center_vy) * (end_y - center_vy)) / (aa * bb);
        double rad = Math.Acos(coss);
        double angle = rad * 180 / Math.PI;

        // 外積チェック
        double gaiseki_check = (m_start_x - center_vx) * (end_y - center_vy) - (m_start_y - center_vy) * (end_x - center_vx);
        if (gaiseki_check < 0) {
            angle = 360 - angle;
        }

        // 右回し
        angle = 360 - angle;
        return (float)angle;
    }

    public static float tmpCalculateAngle(double end_x, double end_y) {
        // 始点、中心点の線分
        double aa = Math.Sqrt((m_start_x - tmpcenter_vx) * (m_start_x - tmpcenter_vx) + (m_start_y - tmpcenter_vy) * (m_start_y - tmpcenter_vy));
        // 終点、中心点の線分
        double bb = Math.Sqrt((end_x - tmpcenter_vx) * (end_x - tmpcenter_vx) + (end_y - tmpcenter_vy) * (end_y - tmpcenter_vy));
        // 角度の算出
        double coss = ((m_start_x - tmpcenter_vx) * (end_x - tmpcenter_vx) + (m_start_y - tmpcenter_vy) * (end_y - tmpcenter_vy)) / (aa * bb);
        double rad = Math.Acos(coss);
        double angle = rad * 180 / Math.PI;

        // 外積チェック
        double gaiseki_check = (m_start_x - tmpcenter_vx) * (end_y - tmpcenter_vy) - (m_start_y - tmpcenter_vy) * (end_x - tmpcenter_vx);
        if (gaiseki_check < 0) {
            angle = 360 - angle;
        }
        // 右回し
        angle = 360 - angle;

        return (float)angle;
    }


    public static bool CalculateCenter(List<GameObject> TraceList, double start_x, double start_y, double end_x, double end_y) {
        if (!iscalCenter) {
            return true;
        }

        min_v = 1000000;
        double distance = CalculateDistance(start_x, start_y, end_x, end_y); // 始点と終点の距離

        if (m_Tracequeue.Count() >= 5) {
            for (int i = 0; i < bunkatu; i++) {
                if (skipcalcenter && !(lastvirtualCenter - calCentarnum <= i && i <= lastvirtualCenter + calCentarnum)) {
                    continue;
                }

                double x1 = 0;
                double y1 = 0;
                double x2 = 0;
                double y2 = 0;
                // 候補円の半径。640*480pixelの画面であったため、候補円の半径を始点終点の距離の半分から240の間で5分割している
                double t = distance / 2 + i * (0.1 - (distance / 2)) / (bunkatu - 1);

                if (distance > 2 * t) {
                    return false; // 始点、終点間の距離が候補円の直径を超える場合は計算しない
                }

                // 仮想中心の設定
                SetVirtualCenter(start_x, start_y, end_x, end_y, t, out x1, out y1, out x2, out y2);

                //---------------ここまでで候補円の中心点計算--------------------

                double v1 = 0;
                double v2 = 0;

                if (m_Tracequeue.Count() >= 5) {
                    CalculateVariancevec2(m_Tracequeue, x1, y1, x2, y2, out v1, out v2);
                } else if (m_Tracequeue.Count() >= 6) { // 間引き処理あり
                    CalculateVariance2(m_Tracequeue, x1, y1, x2, y2, out v1, out v2);
                }

                if (v1 < min_v) { // 分散値が小さい場合意図円の中心点を更新
                    min_v = v1;
                    center_vx = x1;
                    center_vy = y1;
                    tmpvirtualCenter = i;
                }

                if (v2 < min_v) {
                    min_v = v2;
                    center_vx = x2;
                    center_vy = y2;
                    tmpvirtualCenter = i;
                }

                m_VirtualCenterList1.Add(new Vector2((float)x1, (float)y1));
                m_VirtualCenterList2.Add(new Vector2((float)x2, (float)y2));
            }

            //------------------ここまでで候補円の中心点に対する各軌跡の位置の分散を求め、分散が最も小さい点を中心点とする。--------------------
        }

        if (m_Tracequeue.Count() < skipCount) {
            return false;
        }

        return true;
    }

    public static bool tmpCalculateCenter(List<GameObject> TraceList, double start_x, double start_y, double end_x, double end_y) {
        if (!iscalCenter) {
            return true;
        }

        min_v = 1000000;
        double distance = CalculateDistance(start_x, start_y, end_x, end_y);// 始点と終点の距離

        if (m_Tracequeue.Count() >= 5) {
            for (int i = 0; i < bunkatu; i++) {
                if (skipcalcenter && !(lastvirtualCenter - calCentarnum <= i && i <= lastvirtualCenter + calCentarnum)) {
                    continue;
                }

                double x1 = 0;
                double y1 = 0;
                double x2 = 0;
                double y2 = 0;
                // 候補円の半径。640*480pixelの画面であったため、候補円の半径を始点終点の距離の半分から240の間で5分割している
                double t = distance / 2 + i * (0.1 - (distance / 2)) / (bunkatu - 1);


                if (distance > 2 * t) {
                    return false; // 始点、終点間の距離が候補円の直径を超える場合は計算しない
                }

                // 仮想中心の設定
                SetVirtualCenter(start_x, start_y, end_x, end_y, t, out x1, out y1, out x2, out y2);

                //---------------ここまでで候補円の中心点計算--------------------

                double v1 = 0;
                double v2 = 0;

                if (m_Tracequeue.Count() >= 5) {
                    CalculateVariancevec2(m_Tracequeue, x1, y1, x2, y2, out v1, out v2);
                } else if (m_Tracequeue.Count() >= 6) { // 間引き処理あり
                    CalculateVariance2(m_Tracequeue, x1, y1, x2, y2, out v1, out v2);
                }

                if (v1 < min_v) { // 分散値が小さい場合意図円の中心点を更新
                    min_v = v1;
                    tmpcenter_vx = x1;
                    tmpcenter_vy = y1;
                    tmpvirtualCenter = i;
                }

                if (v2 < min_v) {
                    min_v = v2;
                    tmpcenter_vx = x2;
                    tmpcenter_vy = y2;
                    tmpvirtualCenter = i;
                }

                m_VirtualCenterList1.Add(new Vector2((float)x1, (float)y1));
                m_VirtualCenterList2.Add(new Vector2((float)x2, (float)y2));
            }

            //------------------ここまでで候補円の中心点に対する各軌跡の位置の分散を求め、分散が最も小さい点を中心点とする。--------------------
        }

        if (m_Tracequeue.Count() < skipCount) {
            return false;
        }

        return true;
    }
    public static double CalculateDistance(double start_x, double start_y, double end_x, double end_y) {
        return Math.Sqrt((end_x - start_x) * (end_x - start_x) + (end_y - start_y) * (end_y - start_y));
    }

    public static double CalculateXadd(double start_x, double start_y, double end_x, double end_y) {
        return -start_x * start_x * end_x + start_x * start_y * start_y - 2 * end_y * start_x * start_y - start_x * end_x * end_x + end_y * end_y * start_x + start_y * start_y * end_x - 2 * end_y * start_y * end_x + end_x * end_x * end_x + end_y * end_y * end_x;
    }

    public static double CalculateYadd(double start_x, double start_y, double end_x, double end_y) {
        return start_x * start_x * start_y * start_y - end_y * end_y * start_x * start_x - 2 * start_x * start_y * start_y * end_x + 2 * end_y * end_y * start_x * end_x + start_y * start_y * start_y * start_y - 2 * end_y * start_y * start_y * start_y + start_y * start_y * end_x * end_x + 2 * end_y * end_y * end_y * start_y - end_y * end_y * end_x * end_x - end_y * end_y * end_y * end_y;
    }

    public static double CalculateBunbo(double start_x, double start_y, double end_x, double end_y) {
        return start_x * start_x - 2 * start_x * end_x + start_y * start_y - 2 * end_y * start_y + end_x * end_x + end_y * end_y;
    }

    public static double CalculateAngle(double start_x, double start_y, double end_x, double end_y) {
        return start_x * start_x - 2 * start_x * end_x + start_y * start_y - 2 * end_y * start_y + end_x * end_x + end_y * end_y;
    }

    public static void SetVirtualCenter(double start_x, double start_y, double end_x, double end_y, double t, out double x1, out double y1, out double x2, out double y2) {
        x1 = 0;
        y1 = 0;
        x2 = 0;
        y2 = 0;

        double bunbo = CalculateBunbo(start_x, start_y, end_x, end_y);
        double xadd = CalculateXadd(start_x, start_y, end_x, end_y);
        double yadd = CalculateYadd(start_x, start_y, end_x, end_y);

        double root_one = (start_y - end_y) * (start_y - end_y);
        double root_two = bunbo;
        double root_three = (root_two - 4 * t * t);
        double root = Math.Sqrt(-root_one * root_two * root_three);
        double judge = bunbo; // 判別式（一応のため）

        if (start_x == end_x && start_y == end_y) {
            // 計算しない
        } else if (start_y == end_y && start_x != end_x) {
            x1 = (start_x + end_x) / 2;
            y1 = (2 * end_y - Math.Sqrt(-start_x * start_x + 2 * start_x * end_x - end_x * end_x + 4 * t * t)) / 2;
            x2 = x1;
            y2 = (2 * end_y + Math.Sqrt(-start_x * start_x + 2 * start_x * end_x - end_x * end_x + 4 * t * t)) / 2;
        } else if (judge != 0 && start_y != end_y) {
            x1 = (start_x * start_x * start_x - root + xadd) / (2 * bunbo);
            y1 = (start_x * root - end_x * root + yadd) / (2 * (start_y - end_y) * bunbo);
            x2 = (start_x * start_x * start_x + root + xadd) / (2 * bunbo);
            y2 = (-start_x * root + end_x * root + yadd) / (2 * (start_y - end_y) * bunbo);
        } else {
            // 計算しない
        }
    }

    // 分散値計算（一括）
    public static void CalculateVariance2(FixedSizeQueue<Vector2> TraceList, double x1, double y1, double x2, double y2, out double v1, out double v2) {
        double distdanceSum1 = 0;
        double distdanceSquareSum1 = 0;
        double distdanceSum2 = 0;
        double distdanceSquareSum2 = 0;

        int tmp = TraceList.Count(); // 書き終わりまでの履歴点の数;

        int skip = 0;
        int add = 0;

        if (tmp <= 15) {
            //
        } else if (tmp < 30) {
            add = tmp - 15;
        } else {
            skip = 1;
        }

        int addCount = 0 + add;

        int i = 0;
        foreach (var vec2 in TraceList.queue) {
            if (i == addCount) {
                float xtmp = vec2.x; //入力履歴点のj番目のx座標;
                float ytmp = vec2.y;// 入力履歴点のj番目のy座標;
                double distdance1 = Math.Sqrt((x1 - xtmp) * (x1 - xtmp) + (y1 - ytmp) * (y1 - ytmp));//各点と中心点の距離
                distdanceSquareSum1 += distdance1 * distdance1;
                distdanceSum1 += distdance1;//各点と中心点の距離をそれぞれ求めて総和

                double distdance2 = Math.Sqrt((x2 - xtmp) * (x2 - xtmp) + (y2 - ytmp) * (y2 - ytmp));//各点と中心点の距離
                distdanceSquareSum2 += distdance2 * distdance2;
                distdanceSum2 += distdance2;

                addCount = addCount + 1 + skip;
            }
            i++;
        }

        double ave1 = distdanceSum1 / addCount; // 距離平均
        double aveSquare1 = ave1 * ave1;
        v1 = (distdanceSquareSum1 / addCount) - aveSquare1; // 分散の計算
        double ave2 = distdanceSum2 / addCount; // 距離平均
        double aveSquare2 = ave2 * ave2;
        v2 = (distdanceSquareSum2 / addCount) - aveSquare2; // 分散の計算
    }

    // 分散値計算（一括）Vector2
    public static void CalculateVariancevec2(FixedSizeQueue<Vector2> TraceList, double x1, double y1, double x2, double y2, out double v1, out double v2) {
        double distdanceSum1 = 0;
        double distdanceSquareSum1 = 0;
        double distdanceSum2 = 0;
        double distdanceSquareSum2 = 0;

        int tmp = TraceList.Count(); // 書き終わりまでの履歴点の数;

        foreach (var vec2 in TraceList.queue) {
            float xtmp = vec2.x; // 入力履歴点のj番目のx座標;
            float ytmp = vec2.y; // 入力履歴点のj番目のy座標;
            double distdance1 = Math.Sqrt((x1 - xtmp) * (x1 - xtmp) + (y1 - ytmp) * (y1 - ytmp)); // 各点と中心点の距離
            distdanceSquareSum1 += distdance1 * distdance1;
            distdanceSum1 += distdance1; // 各点と中心点の距離をそれぞれ求めて総和

            double distdance2 = Math.Sqrt((x2 - xtmp) * (x2 - xtmp) + (y2 - ytmp) * (y2 - ytmp)); // 各点と中心点の距離
            distdanceSquareSum2 += distdance2 * distdance2;
            distdanceSum2 += distdance2;
        }

        double ave1 = distdanceSum1 / tmp; // 距離平均
        double aveSquare1 = ave1 * ave1;
        v1 = (distdanceSquareSum1 / tmp) - aveSquare1; // 分散の計算
        double ave2 = distdanceSum2 / tmp; // 距離平均
        double aveSquare2 = ave2 * ave2;
        v2 = (distdanceSquareSum2 / tmp) - aveSquare2; // 分散の計算
    }
}

public class CalculateCircleData {
    public bool isCalculated;
    public Vector3 HandPos;
    public float center_x;
    public float center_y;
    public float angle;
    public float angle2;
    public float startAngle;
    public float addAngle;
    public float amountAngle;
    public int rotationNumber;
    public bool isJumped = false;
    public bool isJumpedClockwise;
    public List<Vector2> virtualCenterList1 = new List<Vector2>();
    public List<Vector2> virtualCenterList2 = new List<Vector2>();
}