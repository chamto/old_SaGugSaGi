using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

public class Curves 
{
	//andeeee from the Unity forum's steller Catmull-Rom class ( http://forum.unity3d.com/viewtopic.php?p=218400#218400 ):
	private static Vector3 Interp(Vector3[] pts, float t)
	{
		int numSections = pts.Length - 3;
		int currPt = Mathf.Min(Mathf.FloorToInt(t * (float)numSections), numSections - 1);
		float u = t * (float)numSections - (float)currPt;

		Vector3 a = pts[currPt];
		Vector3 b = pts[currPt + 1];
		Vector3 c = pts[currPt + 2];
		Vector3 d = pts[currPt + 3];

		return .5f * (
			(-a + 3f * b - 3f * c + d) * (u * u * u)
			+ (2f * a - 5f * b + 4f * c - d) * (u * u)
			+ (-a + c) * u
			+ 2f * b
		);
	}

    //ref : https://qroph.github.io/2018/07/30/smooth-paths-using-catmull-rom-splines.html
    //andeeee from the Unity forum's steller Catmull-Rom class ( http://forum.unity3d.com/viewtopic.php?p=218400#218400 ):
    private class CRSpline
	{
		public Vector3[] pts;

		public CRSpline(params Vector3[] pts)
		{
			this.pts = new Vector3[pts.Length];
			Array.Copy(pts, this.pts, pts.Length);
		}


		public Vector3 Interp(float t)
		{
			int numSections = pts.Length - 3;
			int currPt = Mathf.Min(Mathf.FloorToInt(t * (float)numSections), numSections - 1);
			float u = t * (float)numSections - (float)currPt;
			Vector3 a = pts[currPt];
			Vector3 b = pts[currPt + 1];
			Vector3 c = pts[currPt + 2];
			Vector3 d = pts[currPt + 3];
			return .5f * ((-a + 3f * b - 3f * c + d) * (u * u * u) + (2f * a - 5f * b + 4f * c - d) * (u * u) + (-a + c) * u + 2f * b);
		}
	}

    //ref : https://ko.javascript.info/bezier-curve
    //ref : https://m.blog.naver.com/junghs1040/222322521207
    public class Bezier
    {
    
        public Vector3[] positions = null;     // interpolating positions
        public Vector3[] controls = null;      // approximating positions
        public float[] times = null;         // time to arrive at each point
        public float[] lengths = null;       // length of each curve segment
        public float totalLength = 0;   // total length of curve
        public int count = 0;         // number of points and times


        public void Initialize(int in_count)
        {
            // set up arrays
            this.positions = new Vector3[in_count];
            this.controls = new Vector3[2 * (in_count - 1)];
            this.times = new float[in_count];
            this.count = in_count;
        }


        public bool Initialize( Vector3[] in_positions,
                       Vector3[] in_controls,
                       float[] in_times,
                       int in_count )

		{
			//얼마든지 다시 초기화 할 수 있게 한다 
			// make sure not already initialized
			//if (this.count != 0)
			//	return false;

			// make sure data is valid
			if (in_count< 2 || null == in_positions || null == in_times || null == in_controls )
				return false;

			// set up arrays
			this.positions = new Vector3[in_count];
			this.controls = new Vector3[2 * (in_count - 1)];
			this.times = new float[in_count];
			this.count = in_count;

			// copy interpolant data
			uint i;
			for (i = 0; i<in_count; ++i )
			{
				this.positions[i] = in_positions[i];
				this.times[i] = in_times[i];
			}

			// copy approximating control points
			for (i = 0; i< 2*(in_count-1); ++i )
			{
				this.controls[i] = in_controls[i];
			}

			// set up curve segment lengths
			lengths = new float[in_count - 1];
			totalLength = 0.0f;
			for (i = 0; i < in_count - 1; ++i)
			{
				lengths[i] = SegmentArcLength(i, 0.0f, 1.0f);
				totalLength += lengths[i];
			}

			return true;

		}


        public bool Initialize(Vector3[] in_positions, float[] in_times, int in_count )

		{
            //얼마든지 다시 초기화 할 수 있게 한다 
            // make sure not already initialized
            //if (this.count != 0)
				//return false;

			// make sure data is valid
			if (in_count< 2 || null == in_positions || null == in_times )
				return false;

			// set up arrays
			this.positions = new Vector3[in_count];
			controls = new Vector3[2 * (in_count - 1)];
			this.times = new float[in_count];
			this.count = in_count;

			// copy interpolant data
			uint i;
			for (i = 0; i<in_count; ++i )
			{
				this.positions[i] = in_positions[i];
				this.times[i] = in_times[i];
			}

			// create approximating control points
			for (i = 0; i<in_count-1; ++i )
			{
				if (i > 0 )
                    controls[2 * i] = this.positions[i] + (this.positions[i + 1]- this.positions[i - 1])/3.0f;
				if (i<in_count-2 )

                    controls[2 * i + 1] = this.positions[i + 1] - (this.positions[i + 2]- this.positions[i])/3.0f;
			}
            controls[0] = controls[1] - (this.positions[1] - this.positions[0]) / 3.0f;
            controls[2 * in_count - 3] =
            controls[2 * in_count - 4] + (this.positions[in_count - 1] - this.positions[in_count - 2]) / 3.0f;

			// set up curve segment lengths
			lengths = new float[in_count - 1];
			totalLength = 0.0f;
			for (i = 0; i < in_count - 1; ++i)
			{
				lengths[i] = SegmentArcLength(i, 0.0f, 1.0f);
				totalLength += lengths[i];
			}

			return true;

		}

        
        public void SetUp_Approximating_controlPoints()
        {

            if (2 > count) return;

            // create approximating control points
            for (uint i = 0; i < count - 1; ++i)
            {
                if (i > 0)
                    controls[2 * i] = this.positions[i] + (this.positions[i + 1] - this.positions[i - 1]) / 3.0f;
                if (i < count - 2)

                    controls[2 * i + 1] = this.positions[i + 1] - (this.positions[i + 2] - this.positions[i]) / 3.0f;
            }
            controls[0] = controls[1] - (this.positions[1] - this.positions[0]) / 3.0f;
            controls[2 * count - 3] =
            controls[2 * count - 4] + (this.positions[count - 1] - this.positions[count - 2]) / 3.0f;
        }


        public void SetUp_Approximating_controlPoints(float rate)
        {

            if (2 > count) return;

            // create approximating control points
            for (uint i = 0; i < count - 1; ++i)
            {
                if (i > 0)
                    controls[2 * i] = this.positions[i] + (this.positions[i + 1] - this.positions[i - 1]) / rate;
                if (i < count - 2)

                    controls[2 * i + 1] = this.positions[i + 1] - (this.positions[i + 2] - this.positions[i]) / rate;
            }
            controls[0] = controls[1] - (this.positions[1] - this.positions[0]) / rate;
            controls[2 * count - 3] =
            controls[2 * count - 4] + (this.positions[count - 1] - this.positions[count - 2]) / rate;
        }

        public void SetUp_CurveSegmentLengths()
        {
            if (2 > count) return;

            // set up curve segment lengths
            lengths = new float[count - 1];
            totalLength = 0.0f;
            for (uint i = 0; i < count - 1; ++i)
            {
                lengths[i] = SegmentArcLength(i, 0.0f, 1.0f);
                totalLength += lengths[i];
            }

        }

        //-------------------------------------------------------------------------------
        // Evaluate spline
        //-------------------------------------------------------------------------------
        public Vector3 Evaluate(float t)
        {
            // make sure data is valid
            //ASSERT(mCount >= 2);
            if (count < 2)
                return Vector3.zero;

            // handle boundary conditions
            if (t <= times[0])
                return positions[0];
            else if (t >= times[count - 1])
                return positions[count - 1];

            // find segment and parameter
            uint i;
            for (i = 0; i < count - 1; ++i)
            {
                if (t < times[i + 1])
                {
                    break;
                }
            }
            float t0 = times[i];
            float t1 = times[i + 1];
            float u = (t - t0) / (t1 - t0);

            // evaluate
            Vector3 A = positions[i + 1]
                        - 3.0f * controls[2 * i + 1]
                        + 3.0f * controls[2 * i]
                        - positions[i];
            Vector3 B = 3.0f * controls[2 * i + 1]
                        - 6.0f * controls[2 * i]
                        + 3.0f * positions[i];
            Vector3 C = 3.0f * controls[2 * i]
                        - 3.0f * positions[i];

            return positions[i] + u * (C + u * (B + u * A));

        }  


        
        //-------------------------------------------------------------------------------
        // Evaluate spline's derivative
        //-------------------------------------------------------------------------------
        public Vector3 Velocity(float t)
        {
            // make sure data is valid
            //ASSERT(mCount >= 2);
            if (count < 2)
                return Vector3.zero;

            // handle boundary conditions
            if (t <= times[0])
                return positions[0];
            else if (t >= times[count - 1])
                return positions[count - 1];

            // find segment and parameter
            uint i;
            for (i = 0; i < count - 1; ++i)
            {
                if (t < times[i + 1])
                {
                    break;
                }
            }
            float t0 = times[i];
            float t1 = times[i + 1];
            float u = (t - t0) / (t1 - t0);

            // evaluate
            Vector3 A = positions[i + 1]
                        - 3.0f * controls[2 * i + 1]
                        + 3.0f * controls[2 * i]
                        - positions[i];
            Vector3 B = 6.0f * controls[2 * i + 1]
                        - 12.0f * controls[2 * i]
                        + 6.0f * positions[i];
            Vector3 C = 3.0f * controls[2 * i]
                        - 3.0f * positions[i];

            return C + u * (B + 3.0f * u * A);

        }  


        
        //-------------------------------------------------------------------------------
        // Evaluate spline's second derivative
        //-------------------------------------------------------------------------------
        public Vector3 Acceleration(float t)
        {
            // make sure data is valid
            //ASSERT(mCount >= 2);
            if (count < 2)
                return Vector3.zero;

            // handle boundary conditions
            if (t <= times[0])
                return positions[0];
            else if (t >= times[count - 1])
                return positions[count - 1];

            // find segment and parameter
            uint i;
            for (i = 0; i < count - 1; ++i)
            {
                if (t < times[i + 1])
                {
                    break;
                }
            }
            float t0 = times[i];
            float t1 = times[i + 1];
            float u = (t - t0) / (t1 - t0);

            // evaluate
            Vector3 A = positions[i + 1]
                        - 3.0f * controls[2 * i + 1]
                        + 3.0f * controls[2 * i]
                        - positions[i];
            Vector3 B = 6.0f * controls[2 * i + 1]
                        - 12.0f * controls[2 * i]
                        + 6.0f * positions[i];

            return B + 6.0f * u * A;

        }  


        
        //-------------------------------------------------------------------------------
        // Find parameter s distance in arc length from Q(t1)
        // Returns max float if can't find it
        //
        // This extends the approach in the text and uses a mixture of bisection and 
        // Newton-Raphson to find the root.  The result is more stable than Newton-
        // Raphson alone because a) we won't end up with a situation where we divide by 
        // zero in the Newton-Raphson step and b) the end result converges faster.
        //
        // See Numerical Recipes or http://www.essentialmath.com/blog for more details.
        //-------------------------------------------------------------------------------
        public float FindParameterByDistance(float t1, float s)
        {
            // initialize bisection endpoints
            float a = t1;
            float b = times[count - 1];

            // ensure that we remain within valid parameter space
            if (s >= ArcLength(t1, b))
                return b;
            if (s <= 0.0f)
                return a;

            // make first guess
            float p = t1 + s * (times[count - 1] - times[0]) / totalLength;

            // iterate and look for zeros
            for (uint i = 0; i < 32; ++i)
            {
                // compute function value and test against zero
                float func = ArcLength(t1, p) - s;
                if (Mathf.Abs(func) < 1.0e-03f)
                {
                    return p;
                }

                // update bisection endpoints
                if (func < 0.0f)
                {
                    a = p;
                }
                else
                {
                    b = p;
                }

                // get speed along curve
                float speed = Velocity(p).magnitude;

                // if result will lie outside [a,b] 
                if (((p - a) * speed - func) * ((p - b) * speed - func) > -1.0e-3f)
                {
                    // do bisection
                    p = 0.5f * (a + b);
                }
                else
                {
                    // otherwise Newton-Raphson
                    p -= func / speed;
                }
            }

            // done iterating, return failure case
            return float.MaxValue;

        }   


        
        //-------------------------------------------------------------------------------
        // Find length of curve between parameters t1 and t2
        //-------------------------------------------------------------------------------
        public float ArcLength(float t1, float t2)
        {
            if (t2 <= t1)
                return 0.0f;

            if (t1 < times[0])
                t1 = times[0];

            if (t2 > times[count - 1])
                t2 = times[count - 1];

            // find segment and parameter
            uint seg1;
            for (seg1 = 0; seg1 < count - 1; ++seg1)
            {
                if (t1 < times[seg1 + 1])
                {
                    break;
                }
            }
            float u1 = (t1 - times[seg1]) / (times[seg1 + 1] - times[seg1]);

            // find segment and parameter
            uint seg2;
            for (seg2 = 0; seg2 < count - 1; ++seg2)
            {
                if (t2 <= times[seg2 + 1])
                {
                    break;
                }
            }
            float u2 = (t2 - times[seg2]) / (times[seg2 + 1] - times[seg2]);

            float result;
            // both parameters lie in one segment
            if (seg1 == seg2)
            {
                result = SegmentArcLength(seg1, u1, u2);
            }
            // parameters cross segments
            else
            {
                result = SegmentArcLength(seg1, u1, 1.0f);
                for (uint i = seg1 + 1; i < seg2; ++i)
                    result += lengths[i];
                result += SegmentArcLength(seg2, 0.0f, u2);
            }

            return result;

        }   

        
        //-------------------------------------------------------------------------------
        // Find length of curve segment between parameters u1 and u2
        //-------------------------------------------------------------------------------
        public float SegmentArcLength(uint i, float u1, float u2)
        {
            //ASSERT(i >= 0 && i < mCount - 1);

            if (u2 <= u1)
                return 0.0f;

            if (u1 < 0.0f)
                u1 = 0.0f;

            if (u2 > 1.0f)
                u2 = 1.0f;

            Vector3 P0 = positions[i];
            Vector3 P1 = controls[2 * i];
            Vector3 P2 = controls[2 * i + 1];
            Vector3 P3 = positions[i + 1];

            // get control points for subcurve from 0.0 to u2 (de Casteljau's method)
            float minus_u2 = (1.0f - u2);
            Vector3 L1 = minus_u2 * P0 + u2 * P1;
            Vector3 H = minus_u2 * P1 + u2 * P2;
            Vector3 L2 = minus_u2 * L1 + u2 * H;
            Vector3 L3 = minus_u2 * L2 + u2 * (minus_u2 * H + u2 * (minus_u2 * P2 + u2 * P3));

            // resubdivide to get control points for subcurve between u1 and u2
            float minus_u1 = (1.0f - u1);
            H = minus_u1 * L1 + u1 * L2;
            Vector3 R3 = L3;
            Vector3 R2 = minus_u1 * L2 + u1 * L3;
            Vector3 R1 = minus_u1 * H + u1 * R2;
            Vector3 R0 = minus_u1 * (minus_u1 * (minus_u1 * P0 + u1 * L1) + u1 * H) + u1 * R1;

            // get length through subdivision
            return SubdivideLength(in R0, in R1, in R2, in R3);

        }   

        
        //-------------------------------------------------------------------------------
        // Get length of Bezier curve using midpoint subdivision
        //-------------------------------------------------------------------------------
        public float SubdivideLength(in Vector3 P0, in Vector3 P1, in Vector3 P2, in Vector3 P3 )
        {
            // check to see if basically straight
            float Lmin = Vector3.Distance(P0, P3);  
            float Lmax = Vector3.Distance(P0, P1) + Vector3.Distance(P1, P2) + Vector3.Distance(P2, P3);
            float diff = Lmin - Lmax;

            if (diff* diff < 1.0e-3f )
                return 0.5f*(Lmin + Lmax);

            // otherwise get control points for subdivision
            Vector3 L1 = (P0 + P1) * 0.5f;
            Vector3 H = (P1 + P2) * 0.5f;
            Vector3 L2 = (L1 + H) * 0.5f;
            Vector3 R2 = (P2 + P3) * 0.5f;
            Vector3 R1 = (H + R2) * 0.5f;
            Vector3 mid = (L2 + R1) * 0.5f;

            // subdivide
            return SubdivideLength(in P0, in L1, in L2, in mid ) + SubdivideLength(in mid, in R1, in R2, in P3 );

        }   


        //재귀함수로 베지어 곡선 출력 잘안됨 , 사용하지 말기 
        //-------------------------------------------------------------------------------
        // Output points for Bezier curve using midpoint subdivision
        //-------------------------------------------------------------------------------
        public Vector3 SubdivideRender(in Vector3 P0, in Vector3 P1,
                     in Vector3 P2, in Vector3 P3 )
        {
            
            // check to see if straight
            LineSegment3 segment = new LineSegment3(P0, P3 );
            float t;
            if (segment.DistanceSquared(P1, out t) < 1.0e-6f &&
                 segment.DistanceSquared(P2, out t ) < 1.0e-6f )
                 return P0;

            // otherwise get control points for subdivision
            Vector3 L1 = (P0 + P1) * 0.5f; //점p0 과 p1 의 중점 
            Vector3 H = (P1 + P2) * 0.5f;
            Vector3 L2 = (L1 + H) * 0.5f;
            Vector3 R2 = (P2 + P3) * 0.5f;
            Vector3 R1 = (H + R2) * 0.5f;
            Vector3 mid = (L2 + R1) * 0.5f;

            // subdivide
            Vector3 vleft = SubdivideRender(in P0, in L1, in L2, in mid);
            Vector3 vright = SubdivideRender(in mid, in R1, in R2, in P3);

            
            DebugWide.DrawLine(vleft, mid, Color.white);
            DebugWide.DrawLine(vright, mid, Color.white);

            return mid;
        }

        //-------------------------------------------------------------------------------
        // Render spline
        // Uses midpoint subdivision
        //-------------------------------------------------------------------------------
        public void DebugDraw(Color color)
        {

            if (null == positions) return;

            // subdivide to get all but last point
            //Vector3 vlast = Vector3.zero;
            //uint i;
            //for (i = 0; i < mCount - 1; ++i)
            //{
            //    vlast = SubdivideRender(mPositions[i], mControls[2 * i], mControls[2 * i + 1], mPositions[i + 1]);
            //}
            //// output last point
            //DebugWide.DrawLine(vlast, mPositions[mCount - 1], Color.white);


            Vector3 beforeEva = positions[0];
            Vector3 afterEva = beforeEva;
            float max_t = times[count - 1];
            float t = 0;
            int maxdraw = 30;
            for (int i = 0; i < maxdraw + 1; ++i)
            {
                
                t = (i / (float)maxdraw) * max_t;

                afterEva = Evaluate(t);
                DebugWide.DrawLine(beforeEva, afterEva , color);

                beforeEva = afterEva;
            }

            
            // draw sample points
            for (int i = 0; i < count; ++i)
            {
                DebugWide.DrawCircle(positions[i], 0.15f, color);
                
            }

            for (int i = 0; i < count-1; ++i)
            {
                DebugWide.DrawLine(positions[i], controls[i * 2], color);
                DebugWide.DrawLine(controls[i * 2], controls[i * 2 + 1], color);
                DebugWide.DrawLine(controls[i * 2 + 1], positions[i + 1], color);
            }

            // draw control points
            for (int i = 0; i < 2 * count - 2; ++i)
            {
                DebugWide.DrawCircle(controls[i], 0.05f, color);

            }

        }

        public void TestCode()
        {
            Vector3[] samplePositions = new Vector3[4];
            samplePositions[0].Set( -6.0f, 3.0f, 0.0f );
            samplePositions[1].Set( 0.0f, 0.0f, 0.0f );
            samplePositions[2].Set( -3.0f, -3.0f, 0.0f );
            samplePositions[3].Set( 6.0f, 0.0f, 0.0f );

            Vector3[] sampleControls = new Vector3[6];
            sampleControls[0].Set( -3.0f, 3.0f, 0.0f );
            sampleControls[1].Set( -0.5f, 1.5f, 0.0f );
            sampleControls[2].Set( 0.5f, -1.5f, 0.0f );
            sampleControls[3].Set( -2.0f, -1.5f, 0.0f );
            sampleControls[4].Set( 0.0f, -5.0f, 0.0f );
            sampleControls[5].Set( 4.0f, -3.0f, 0.0f );

            float[] sampleTimes = new float[4];
            sampleTimes[0] = 0.0f;
            sampleTimes[1] = 2.0f;
            sampleTimes[2] = 6.0f;
            sampleTimes[3] = 9.0f;

            this.Initialize(samplePositions, sampleControls, sampleTimes, 4 );
            //this.Initialize(samplePositions, sampleTimes, 4); //자동으로 컨트롤 생성 
        }


        public void TestCode2()
        {
            Vector3[] samplePositions = new Vector3[3];
            samplePositions[0].Set(1.0f, 0.0f, 0.0f);
            samplePositions[1].Set(3.0f, 1.9f, 0.0f);
            samplePositions[2].Set(5.0f, 0.0f, 0.0f);
            
            
            float[] sampleTimes = new float[3];
            sampleTimes[0] = 0.0f;
            sampleTimes[1] = 2.0f;
            sampleTimes[2] = 6.0f;
            

            this.Initialize(samplePositions, sampleTimes, 3); //자동으로 컨트롤 생성 
        }

    }//end Bezier

    //==========================================================================
    //==========================================================================
    //==========================================================================

    public class CatmullRom
    {
        public Vector3[] positions = null;     // sample positions
        public float[] times = null;         // time to arrive at each point
        public float[] lengths = null;       // length of each curve segment
        public float totalLength = 0;   // total length of curve
        public int count = 0;         // number of points and times


        public bool Initialize_NonAlloc(Vector3[] in_positions, float[] in_times, int in_count)
        {
            // make sure data is valid
            if (in_count < 4 || null == in_positions || null == in_times)
                return false;

            // set up arrays
            this.positions = in_positions;
            this.times = in_times;
            this.count = in_count;


            // set up curve segment lengths
            lengths = new float[in_count - 1];
            totalLength = 0.0f;
            for (int i = 0; i < in_count - 1; ++i)
            {
                lengths[i] = SegmentArcLength(i, 0.0f, 1.0f);
                totalLength += lengths[i];
            }

            return true;
        }

        public bool Initialize(Vector3[] in_positions, float[] in_times, int in_count)
        {
            //얼마든지 다시 초기화 할 수 있게 한다 
            // make sure not already initialized
            //if (this.count != 0)
            //    return false;

            // make sure data is valid
            if (in_count < 4 || null == in_positions || null == in_times)
                return false;

            // set up arrays
            this.positions = new Vector3[in_count];
            this.times = new float[in_count];
            this.count = in_count;

            // copy data
            int i;
            for (i = 0; i < in_count; ++i)
            {
                this.positions[i] = in_positions[i];
                this.times[i] = in_times[i];
            }

            // set up curve segment lengths
            lengths = new float[in_count - 1];
            totalLength = 0.0f;
            for (i = 0; i < in_count - 1; ++i)
            {
                lengths[i] = SegmentArcLength(i, 0.0f, 1.0f);
                totalLength += lengths[i];
            }

            return true;

        }



        //-------------------------------------------------------------------------------
        // @ CatmullRom::Evaluate()
        //-------------------------------------------------------------------------------
        // Evaluate spline
        //-------------------------------------------------------------------------------
        public Vector3 Evaluate(float t)
        {
            // make sure data is valid
            //ASSERT(mCount >= 2);
            if (count < 2)
                return Vector3.zero;

            // handle boundary conditions
            if (t <= times[0])
                return positions[0];
            else if (t >= times[count - 1])
                return positions[count - 1];

            // find segment and parameter
            int i;  // segment #
            for (i = 0; i < count - 1; ++i)
            {
                if (t <= times[i + 1])
                {
                    break;
                }
            }
            //ASSERT(i >= 0 && i < mCount);

            float t0 = times[i];
            float t1 = times[i + 1];
            float u = (t - t0) / (t1 - t0);

            // quadratic Catmull-Rom for Q_0
            if (i == 0)
            {
                Vector3 A = positions[0] - 2.0f * positions[1] + positions[2];
                Vector3 B = 4.0f * positions[1] - 3.0f * positions[0] - positions[2];

                return positions[0] + (0.5f * u) * (B + u * A);
            }
            // quadratic Catmull-Rom for Q_n-1
            else if (i >= count - 2)
            {
                i = count - 2;
                Vector3 A = positions[i - 1] - 2.0f * positions[i] + positions[i + 1];
                Vector3 B = positions[i + 1] - positions[i - 1];

                return positions[i] + (0.5f * u) * (B + u * A);
            }
            // cubic Catmull-Rom for interior segments
            else
            {
                // evaluate
                Vector3 A = 3.0f * positions[i]
                            - positions[i - 1]
                            - 3.0f * positions[i + 1]
                            + positions[i + 2];
                Vector3 B = 2.0f * positions[i - 1]
                            - 5.0f * positions[i]
                            + 4.0f * positions[i + 1]
                            - positions[i + 2];
                Vector3 C = positions[i + 1] - positions[i - 1];

                return positions[i] + (0.5f * u) * (C + u * (B + u * A));
            }

        }   // End of CatmullRom::Evaluate()


        //-------------------------------------------------------------------------------
        // @ CatmullRom::Velocity()
        //-------------------------------------------------------------------------------
        // Evaluate derivative at parameter t
        //-------------------------------------------------------------------------------
        public Vector3 Velocity(float t)
        {
            // make sure data is valid
            //ASSERT(mCount >= 2);
            if (count < 2)
                return Vector3.zero;

            // handle boundary conditions
            if (t <= times[0])
                t = 0.0f;
            else if (t > times[count - 1])
                t = times[count - 1];

            // find segment and parameter
            int i;
            for (i = 0; i < count - 1; ++i)
            {
                if (t <= times[i + 1])
                {
                    break;
                }
            }
            float t0 = times[i];
            float t1 = times[i + 1];
            float u = (t - t0) / (t1 - t0);

            // evaluate
            // quadratic Catmull-Rom for Q_0
            if (i == 0)
            {
                Vector3 A = positions[0] - 2.0f * positions[1] + positions[2];
                Vector3 B = 4.0f * positions[1] - 3.0f * positions[0] - positions[2];

                return 0.5f * B + u * A;
            }
            // quadratic Catmull-Rom for Q_n-1
            else if (i >= count - 2)
            {
                i = count - 2;
                Vector3 A = positions[i - 1] - 2.0f * positions[i] + positions[i + 1];
                Vector3 B = positions[i + 1] - positions[i - 1];

                return 0.5f * B + u * A;
            }
            // cubic Catmull-Rom for interior segments
            else
            {
                // evaluate
                Vector3 A = 3.0f * positions[i]
                            - positions[i - 1]
                            - 3.0f * positions[i + 1]
                            + positions[i + 2];
                Vector3 B = 2.0f * positions[i - 1]
                            - 5.0f * positions[i]
                            + 4.0f * positions[i + 1]
                            - positions[i + 2];
                Vector3 C = positions[i + 1] - positions[i - 1];

                return 0.5f * C + u * (B + 1.5f * u * A);
            }

        }   // End of CatmullRom::Velocity()


        //-------------------------------------------------------------------------------
        // @ CatmullRom::Acceleration()
        //-------------------------------------------------------------------------------
        // Evaluate second derivative at parameter t
        //-------------------------------------------------------------------------------
        public Vector3 Acceleration(float t)
        {
            // make sure data is valid
            //ASSERT(mCount >= 2);
            if (count < 2)
                return Vector3.zero;

            // handle boundary conditions
            if (t <= times[0])
                t = 0.0f;
            else if (t > times[count - 1])
                t = times[count - 1];

            // find segment and parameter
            int i;
            for (i = 0; i < count - 1; ++i)
            {
                if (t <= times[i + 1])
                {
                    break;
                }
            }
            float t0 = times[i];
            float t1 = times[i + 1];
            float u = (t - t0) / (t1 - t0);

            // evaluate
            // quadratic Catmull-Rom for Q_0
            if (i == 0)
            {
                return positions[0] - 2.0f * positions[1] + positions[2];
            }
            // quadratic Catmull-Rom for Q_n-1
            else if (i >= count - 2)
            {
                i = count - 2;
                return positions[i - 1] - 2.0f * positions[i] + positions[i + 1];
            }
            // cubic Catmull-Rom for interior segments
            else
            {
                // evaluate
                Vector3 A = 3.0f * positions[i]
                            - positions[i - 1]
                            - 3.0f * positions[i + 1]
                            + positions[i + 2];
                Vector3 B = 2.0f * positions[i - 1]
                            - 5.0f * positions[i]
                            + 4.0f * positions[i + 1]
                            - positions[i + 2];

                return B + (3.0f * u) * A;
            }

        }   // End of CatmullRom::Acceleration()


        //-------------------------------------------------------------------------------
        // @ CatmullRom::FindParameterByDistance()
        //-------------------------------------------------------------------------------
        // Find parameter s distance in arc length from Q(t1)
        // Returns max float if can't find it
        //
        // This extends the approach in the text and uses a mixture of bisection and 
        // Newton-Raphson to find the root.  The result is more stable than Newton-
        // Raphson alone because a) we won't end up with a situation where we divide by 
        // zero in the Newton-Raphson step and b) the end result converges faster.
        //
        // See Numerical Recipes or http://www.essentialmath.com/blog for more details.
        //-------------------------------------------------------------------------------
        public float FindParameterByDistance(float t1, float s)
        {
            // initialize bisection endpoints
            float a = t1;
            float b = times[count - 1];

            // ensure that we remain within valid parameter space
            if (s >= ArcLength(t1, b))
                return b;
            if (s <= 0.0f)
                return a;

            // make first guess
            float p = t1 + s * (times[count - 1] - times[0]) / totalLength;

            // iterate and look for zeros
            for (int i = 0; i < 32; ++i)
            {
                // compute function value and test against zero
                float func = ArcLength(t1, p) - s;
                if ( Mathf.Abs(func) < 1.0e-03f)
                {
                    return p;
                }

                // update bisection endpoints
                if (func < 0.0f)
                {
                    a = p;
                }
                else
                {
                    b = p;
                }

                // get speed along curve
                float speed = Velocity(p).magnitude;

                // if result will lie outside [a,b] 
                if (((p - a) * speed - func) * ((p - b) * speed - func) > -1.0e-3f)
                {
                    // do bisection
                    p = 0.5f * (a + b);
                }
                else
                {
                    // otherwise Newton-Raphson
                    p -= func / speed;
                }
            }

            // done iterating, return failure case
            return float.MaxValue;

        }   // End of CatmullRom::FindParameterByDistance()


        //-------------------------------------------------------------------------------
        // @ CatmullRom::ArcLength()
        //-------------------------------------------------------------------------------
        // Find length of curve between parameters t1 and t2
        //-------------------------------------------------------------------------------
        public float ArcLength(float t1, float t2)
        {
            if (t2 <= t1)
                return 0.0f;

            if (t1 < times[0])
                t1 = times[0];

            if (t2 > times[count - 1])
                t2 = times[count - 1];

            // find segment and parameter
            int seg1;
            for (seg1 = 0; seg1 < count - 1; ++seg1)
            {
                if (t1 <= times[seg1 + 1])
                {
                    break;
                }
            }
            float u1 = (t1 - times[seg1]) / (times[seg1 + 1] - times[seg1]);

            // find segment and parameter
            int seg2;
            for (seg2 = 0; seg2 < count - 1; ++seg2)
            {
                if (t2 <= times[seg2 + 1])
                {
                    break;
                }
            }
            float u2 = (t2 - times[seg2]) / (times[seg2 + 1] - times[seg2]);

            float result;
            // both parameters lie in one segment
            if (seg1 == seg2)
            {
                result = SegmentArcLength(seg1, u1, u2);
            }
            // parameters cross segments
            else
            {
                result = SegmentArcLength(seg1, u1, 1.0f);
                for (int i = seg1 + 1; i < seg2; ++i)
                    result += lengths[i];
                result += SegmentArcLength(seg2, 0.0f, u2);
            }

            return result;

        }   // End of CatmullRom::ArcLength()


        //-------------------------------------------------------------------------------
        // @ CatmullRom::SegmentArcLength()
        //-------------------------------------------------------------------------------
        // Find length of curve segment between parameters u1 and u2
        //-------------------------------------------------------------------------------
        public float SegmentArcLength(int i, float u1, float u2)
        {
            float[] x =
            {
                0.0000000000f, 0.5384693101f, -0.5384693101f, 0.9061798459f, -0.9061798459f
            };

            float[] c =
            {
                0.5688888889f, 0.4786286705f, 0.4786286705f, 0.2369268850f, 0.2369268850f
            };

            //ASSERT(i >= 0 && i < mCount - 1);

            if (u2 <= u1)
                return 0.0f;

            if (u1 < 0.0f)
                u1 = 0.0f;

            if (u2 > 1.0f)
                u2 = 1.0f;

            // use Gaussian quadrature
            float sum = 0.0f;
            Vector3 A, B, C;
            C = Vector3.zero;

            if (i == 0)
            {
                A = positions[0] - 2.0f * positions[1] + positions[2];
                B = 4.0f * positions[1] - 3.0f * positions[0] - positions[2];

            }
            // quadratic Catmull-Rom for Q_n-1
            else if (i >= count - 2)
            {
                i = count - 2;
                A = positions[i - 1] - 2.0f * positions[i] + positions[i + 1];
                B = positions[i + 1] - positions[i - 1];
            }
            // cubic Catmull-Rom for interior segments
            else
            {
                A = 3.0f * positions[i]
                            - positions[i - 1]
                            - 3.0f * positions[i + 1]
                            + positions[i + 2];
                B = 2.0f * positions[i - 1]
                            - 5.0f * positions[i]
                            + 4.0f * positions[i + 1]
                            - positions[i + 2];
                C = positions[i + 1] - positions[i - 1];
            }

            for (int j = 0; j < 5; ++j)
            {
                float u = 0.5f * ((u2 - u1) * x[j] + u2 + u1);
                Vector3 derivative;
                if (i == 0 || i >= count - 2)
                    derivative = 0.5f * B + u * A;
                else
                    derivative = 0.5f * C + u * (B + 1.5f * u * A);
                sum += c[j] * derivative.magnitude;
            }
            sum *= 0.5f * (u2 - u1);

            return sum;

        }   // End of CatmullRom::SegmentArcLength()

        public void DebugDraw(Color color)
        {
            if (null == positions) return;

            Vector3 beforeEva = positions[0];
            Vector3 afterEva = beforeEva;
            float max_t = times[count - 1];
            float t = 0;
            int maxdraw = 30;
            for (int i = 0; i < maxdraw + 1; ++i)
            {

                t = (i / (float)maxdraw) * max_t;

                afterEva = Evaluate(t);
                DebugWide.DrawLine(beforeEva, afterEva, color);

                beforeEva = afterEva;
            }


            // draw sample points
            for (int i = 0; i < count; ++i)
            {
                DebugWide.DrawCircle(positions[i], 0.15f, color);

            }

        }

        public void TestCode()
        {
            Vector3[] samplePositions = new Vector3[4];
            samplePositions[0].Set(-6.0f, 3.0f, 0.0f);
            samplePositions[1].Set(0.0f, 0.0f, 0.0f);
            samplePositions[2].Set(-3.0f, -3.0f, 0.0f);
            samplePositions[3].Set(6.0f, 0.0f, 0.0f);

            
            float[] sampleTimes = new float[4];
            sampleTimes[0] = 0.0f;
            sampleTimes[1] = 2.0f;
            sampleTimes[2] = 6.0f;
            sampleTimes[3] = 9.0f;

            this.Initialize(samplePositions, sampleTimes, 4);
            
        }

    }//end class
}
