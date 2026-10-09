using AT.cx.NET;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Alp.Com.VisionSystem.ATProfilerHalcon.Configurazioni
{
    class CommonProfilometri
    {
        // Classe creata per uso da AT.cx.NET, quindi per il funzionamento del profilometro e per la resa delle scansioni

        public static cx_pixel_format cv2pf(DepthType cvType)
        {
            cx_pixel_format pf = cx_pixel_format.CX_PF_MONO_8;
            switch (cvType)
            {
                case DepthType.Cv8U: pf = cx_pixel_format.CX_PF_MONO_8; break;
                case DepthType.Cv16U: pf = cx_pixel_format.CX_PF_MONO_16; break;
                case DepthType.Cv32S: pf = cx_pixel_format.CX_PF_MONO_32; break;
                case DepthType.Cv32F: pf = cx_pixel_format.CX_PF_COORD3D_C32f; break;
                default:
                    throw (new ArgumentException("Invalid Parameter"));
            }
            return pf;
        }

        public static DepthType Pf2depth(uint pixelFormat)
        {
            DepthType cvType;
            switch (pixelFormat)
            {
                case (uint)cx_pixel_format.CX_PF_MONO_8:
                case (uint)cx_pixel_format.CX_PF_COORD3D_C8: cvType = DepthType.Cv8U; break;

                case (uint)cx_pixel_format.CX_PF_COORD3D_ABC8:
                case (uint)cx_pixel_format.CX_PF_COORD3D_ABC8_PLANAR: cvType = DepthType.Cv8U; break;

                case (uint)cx_pixel_format.CX_PF_MONO_10:
                case (uint)cx_pixel_format.CX_PF_MONO_10p:
                case (uint)cx_pixel_format.CX_PF_MONO_12:
                case (uint)cx_pixel_format.CX_PF_MONO_12p:
                case (uint)cx_pixel_format.CX_PF_MONO_14:
                case (uint)cx_pixel_format.CX_PF_MONO_16:
                case (uint)cx_pixel_format.CX_PF_COORD3D_C12p:
                case (uint)cx_pixel_format.CX_PF_COORD3D_C16: cvType = DepthType.Cv16U; break;

                case (uint)cx_pixel_format.CX_PF_COORD3D_ABC16:
                case (uint)cx_pixel_format.CX_PF_COORD3D_ABC16_PLANAR: cvType = DepthType.Cv16U; break;

                case (uint)cx_pixel_format.CX_PF_MONO_32: cvType = DepthType.Cv32S; break;

                case (uint)cx_pixel_format.CX_PF_COORD3D_C32f: cvType = DepthType.Cv32F; break;

                case (uint)cx_pixel_format.CX_PF_COORD3D_ABC32f:
                case (uint)cx_pixel_format.CX_PF_COORD3D_ABC32f_PLANAR: cvType = DepthType.Cv32F; break;
                default:
                    throw (new ArgumentException("Invalid Parameter"));
            }
            return cvType;
        }

        public static DepthType Pf2depth(string pixelFormat)
        {
            DepthType cvType;
            if (pixelFormat == cxBase.CX_PF_MONO_8_STR || pixelFormat == cxBase.CX_PF_COORD3D_C8_STR) cvType = DepthType.Cv8U;
            else if (pixelFormat == cxBase.CX_PF_COORD3D_ABC8_STR || pixelFormat == cxBase.CX_PF_COORD3D_ABC8_PLANAR_STR) cvType = DepthType.Cv8U;
            else if (pixelFormat == cxBase.CX_PF_MONO_10_STR ||
                      pixelFormat == cxBase.CX_PF_MONO_10p_STR ||
                      pixelFormat == cxBase.CX_PF_MONO_12_STR ||
                      pixelFormat == cxBase.CX_PF_MONO_12p_STR ||
                      pixelFormat == cxBase.CX_PF_MONO_14_STR ||
                      pixelFormat == cxBase.CX_PF_MONO_16_STR ||
                      pixelFormat == cxBase.CX_PF_COORD3D_C12p_STR ||
                      pixelFormat == cxBase.CX_PF_COORD3D_C16_STR
                      ) cvType = DepthType.Cv16U;
            else if (pixelFormat == cxBase.CX_PF_COORD3D_ABC16_STR || pixelFormat == cxBase.CX_PF_COORD3D_ABC16_PLANAR_STR) cvType = DepthType.Cv16U;
            else if (pixelFormat == cxBase.CX_PF_MONO_32_STR) cvType = DepthType.Cv32S;
            else if (pixelFormat == cxBase.CX_PF_COORD3D_C32f_STR) cvType = DepthType.Cv32F;
            else if (pixelFormat == cxBase.CX_PF_COORD3D_ABC32f_STR || pixelFormat == cxBase.CX_PF_COORD3D_ABC32f_PLANAR_STR) cvType = DepthType.Cv32F;
            else
            {
                throw (new ArgumentException("pixelFormat", pixelFormat));
            }
            return cvType;
        }

        public static int Pf2channels(uint pixelFormat)
        {
            int channels;
            switch (pixelFormat)
            {
                case (uint)cx_pixel_format.CX_PF_MONO_8:
                case (uint)cx_pixel_format.CX_PF_COORD3D_C8: channels = 1; break;

                case (uint)cx_pixel_format.CX_PF_COORD3D_ABC8:
                case (uint)cx_pixel_format.CX_PF_COORD3D_ABC8_PLANAR: channels = 3; break;

                case (uint)cx_pixel_format.CX_PF_MONO_10:
                case (uint)cx_pixel_format.CX_PF_MONO_10p:
                case (uint)cx_pixel_format.CX_PF_MONO_12:
                case (uint)cx_pixel_format.CX_PF_MONO_12p:
                case (uint)cx_pixel_format.CX_PF_MONO_14:
                case (uint)cx_pixel_format.CX_PF_MONO_16:
                case (uint)cx_pixel_format.CX_PF_COORD3D_C12p:
                case (uint)cx_pixel_format.CX_PF_COORD3D_C16: channels = 1; break;

                case (uint)cx_pixel_format.CX_PF_COORD3D_ABC16:
                case (uint)cx_pixel_format.CX_PF_COORD3D_ABC16_PLANAR: channels = 3; break;

                case (uint)cx_pixel_format.CX_PF_MONO_32: channels = 1; break;

                case (uint)cx_pixel_format.CX_PF_COORD3D_C32f: channels = 1; break;

                case (uint)cx_pixel_format.CX_PF_COORD3D_ABC32f:
                case (uint)cx_pixel_format.CX_PF_COORD3D_ABC32f_PLANAR: channels = 3; break;
                default:
                    throw (new ArgumentException("Invalid Parameter"));
            }
            return channels;
        }

        public static int Pf2channels(string pixelFormat)
        {
            int channels;
            if (pixelFormat == cxBase.CX_PF_MONO_8_STR || pixelFormat == cxBase.CX_PF_COORD3D_C8_STR) channels = 1;
            else if (pixelFormat == cxBase.CX_PF_COORD3D_ABC8_STR || pixelFormat == cxBase.CX_PF_COORD3D_ABC8_PLANAR_STR) channels = 3;
            else if (pixelFormat == cxBase.CX_PF_MONO_10_STR ||
                        pixelFormat == cxBase.CX_PF_MONO_10p_STR ||
                        pixelFormat == cxBase.CX_PF_MONO_12_STR ||
                        pixelFormat == cxBase.CX_PF_MONO_12p_STR ||
                        pixelFormat == cxBase.CX_PF_MONO_14_STR ||
                        pixelFormat == cxBase.CX_PF_MONO_16_STR ||
                        pixelFormat == cxBase.CX_PF_COORD3D_C12p_STR ||
                        pixelFormat == cxBase.CX_PF_COORD3D_C16_STR
                        ) channels = 1;
            else if (pixelFormat == cxBase.CX_PF_COORD3D_ABC16_STR || pixelFormat == cxBase.CX_PF_COORD3D_ABC16_PLANAR_STR) channels = 3;
            else if (pixelFormat == cxBase.CX_PF_MONO_32_STR) channels = 1;
            else if (pixelFormat == cxBase.CX_PF_COORD3D_C32f_STR) channels = 1;
            else if (pixelFormat == cxBase.CX_PF_COORD3D_ABC32f_STR || pixelFormat == cxBase.CX_PF_COORD3D_ABC32f_PLANAR_STR) channels = 3;
            else
            {
                throw (new ArgumentException("pixelFormat", pixelFormat));
            }
            return channels;
        }

        public static string partTypeIDToStr(cx_buffer_part_type typeID)
        {
            string strTypeID = "";

            if (typeID == cx_buffer_part_type.CX_BUFFER_PART_TYPE_ID_UNDEFINED) strTypeID = "CX_BUFFER_PART_TYPE_ID_UNDEFINED";
            else if (typeID == cx_buffer_part_type.CX_BUFFER_PART_TYPE_ID_IMAGE2D) strTypeID = "CX_BUFFER_PART_TYPE_ID_IMAGE2D";
            else if (typeID == cx_buffer_part_type.CX_BUFFER_PART_TYPE_ID_TOKEN) strTypeID = "CX_BUFFER_PART_TYPE_ID_TOKEN";
            else if (typeID == cx_buffer_part_type.CX_BUFFER_PART_TYPE_ID_CHUNK) strTypeID = "CX_BUFFER_PART_TYPE_ID_CHUNK";
            else if (typeID == cx_buffer_part_type.CX_BUFFER_PART_TYPE_ID_CALIBIR) strTypeID = "CX_BUFFER_PART_TYPE_ID_CALIBIR";
            else if (typeID == cx_buffer_part_type.CX_BUFFER_PART_TYPE_ID_CALIB3D) strTypeID = "CX_BUFFER_PART_TYPE_ID_CALIB3D";
            else if (typeID == cx_buffer_part_type.CX_BUFFER_PART_TYPE_ID_CONFIG) strTypeID = "CX_BUFFER_PART_TYPE_ID_CONFIG";
            else strTypeID = "Unknown";

            return strTypeID;
        }

        public static string partPurposeIDToStr(cx_buffer_part_purpose purposeID)
        {
            string strPurposeID = "";

            if (purposeID == cx_buffer_part_purpose.CX_BUFFER_PART_PURPOSE_ID_UNDEFINED) strPurposeID = "CX_BUFFER_PART_PURPOSE_ID_UNDEFINED";
            else if (purposeID == cx_buffer_part_purpose.CX_BUFFER_PART_PURPOSE_ID_INTENSITY) strPurposeID = "CX_BUFFER_PART_PURPOSE_ID_INTENSITY";
            else if (purposeID == cx_buffer_part_purpose.CX_BUFFER_PART_PURPOSE_ID_INFRARED) strPurposeID = "CX_BUFFER_PART_PURPOSE_ID_INFRARED";
            else if (purposeID == cx_buffer_part_purpose.CX_BUFFER_PART_PURPOSE_ID_RANGE) strPurposeID = "CX_BUFFER_PART_PURPOSE_ID_RANGE";
            else if (purposeID == cx_buffer_part_purpose.CX_BUFFER_PART_PURPOSE_ID_REFLECTANCE) strPurposeID = "CX_BUFFER_PART_PURPOSE_ID_REFLECTANCE";
            else if (purposeID == cx_buffer_part_purpose.CX_BUFFER_PART_PURPOSE_ID_CONFIDENCE) strPurposeID = "CX_BUFFER_PART_PURPOSE_ID_CONFIDENCE";
            else if (purposeID == cx_buffer_part_purpose.CX_BUFFER_PART_PURPOSE_ID_SCATTER) strPurposeID = "CX_BUFFER_PART_PURPOSE_ID_SCATTER";
            else if (purposeID == cx_buffer_part_purpose.CX_BUFFER_PART_PURPOSE_ID_RANGE_AT_TOKEN) strPurposeID = "CX_BUFFER_PART_PURPOSE_ID_RANGE_AT_TOKEN";
            else if (purposeID == cx_buffer_part_purpose.CX_BUFFER_PART_PURPOSE_ID_METADATA) strPurposeID = "CX_BUFFER_PART_PURPOSE_ID_METADATA";
            else strPurposeID = "Unknown";

            return strPurposeID;
        }

        // TODO: FASE 2 - Da studiare cosa fare quando ci sono le calib e tutto
        public static void downloadCalib(Device cam, Calib calib)
        {
            bool bErrorDownloadingFactoryCalib = false;
            /*try
            {
                string buffer;
                buffer = cam.downloadFileToBuffer("CalibrationFactory");
                calib.loadFromBuffer(buffer);
            }
            catch (Exception e)
            {
                bErrorDownloadingFactoryCalib = true;
            }*/

            if (!bErrorDownloadingFactoryCalib)
            {
                try
                {
                    string buffer;
                    buffer = cam.downloadFileToBuffer("UserData");
                    calib.loadFromBuffer(buffer);
                }
                catch (Exception e)
                {
                    Console.WriteLine("downloadCalib, " + e.Message);
                    bErrorDownloadingFactoryCalib = true;
                }
            }
        }

        // TODO: FASE 2 - Da studiare meglio come viene updatata direttamente sul profilometro
        public static void updateCalibC6(Device cam, Calib calib, int regionId)
        {
            Variant val = 0;

            // set sensor size parameter from camera (requires for LUT_mode, binary calibration format does not include this parameter)
            System.Int64[] s_size = new System.Int64[2] { 0, 0 };
            cam.getParam("SensorWidth", val);
            s_size[0] = val;
            cam.getParam("SensorHeight", val);
            s_size[1] = val;
            val.set(s_size);
            calib.setParam(cx_3d_calib_param_t.CX_3D_PARAM_S_SZ, val);

            // set horizontal resolution reduction caused by sensor binning or decimation
            uint rrh = 1;
            try
            {
                cam.getParam("BinningHorizontal", val);
                if (((uint)val) > 0)
                    rrh *= val;
            }
            catch (Exception e) { }
            try
            {
                cam.getParam("DecimationHorizontal", val);
                if (((uint)val) > 0)
                    rrh *= val;
            }
            catch (Exception e) { }
            val.set(rrh);
            calib.setParam(cx_3d_calib_param_t.CX_3D_PARAM_S_RR_H, val);

            // set vertical resolution reduction caused by sensor binning or decimation
            uint rrv = 1;
            try
            {
                cam.getParam("BinningVertical", val);
                if (((uint)val) > 0)
                    rrv *= val;
            }
            catch (Exception e) { }
            try
            {
                cam.getParam("DecimationVertical", val);
                if (((uint)val) > 0)
                    rrv *= val;
            }
            catch (Exception e) { }
            val.set(rrv);
            calib.setParam(cx_3d_calib_param_t.CX_3D_PARAM_S_RR_V, val);

            // seclect extraction module belonging to given AOI
            long[] enumVals = { };
            val.get(ref enumVals);

            foreach (long enumVal in enumVals)
            {
                val.set(enumVal);
                cam.setParam("Scan3dExtractionSelector", val);

                cam.getParamInfo(cx_param_info.CX_PARAM_INFO_ENUM_INT_VALUE, "Scan3dExtractionSource", val);

                if ((long)val == regionId)
                {
                    cam.getParam("Scan3dCoordinateScale", val);
                    val = (double)val;
                    calib.setParam(cx_3d_calib_param_t.CX_3D_PARAM_RANGE_SCALE, val);

                    cam.getParam("Scan3dCoordinateOffset", val);
                    calib.setParam(cx_3d_calib_param_t.CX_3D_PARAM_RANGE_OFFSET, val);
                }
            }

            // set X offset in image space
            cam.getParam(cxCam.CX_CAM_IMAGE_OFFSET_X, val);
            calib.setParam(cx_3d_calib_param_t.CX_3D_PARAM_S_ROI_X, val);

            cam.getParam("RegionSelector", val);
            String storedRegionId = val;

            // select the AOI on the camera we want to read the parameter for
            cam.setParam("RegionSelector", new Variant(regionId));

            // set image width, if reverse-x is true set negative
            bool reverseX = false;
            try
            {
                cam.getParam(cxCam.CX_CAM_IMAGE_REVERSE_X, val);
                reverseX = ((int)val == 0) ? false : true;
            }
            catch (Exception e) { }

            cam.getParam(cxCam.CX_CAM_IMAGE_WIDTH, val);
            int width = val;
            if (reverseX)
                width = -width;
            calib.setParam(cx_3d_calib_param_t.CX_3D_PARAM_S_ROI_W, new Variant(width));

            // set AOI height, if reverse-y is true set negative
            bool reverseY = false;
            try
            {
                cam.getParam(cxCam.CX_CAM_IMAGE_REVERSE_Y, val);
                reverseY = ((int)val == 0) ? false : true;
            }
            catch (Exception e) { }

            cam.getParam("Height", val);
            int roiHeight = val;
            if (reverseY)
                roiHeight = -roiHeight;
            calib.setParam(cx_3d_calib_param_t.CX_3D_PARAM_S_ROI_H, new Variant(roiHeight));

            cam.setParam("RegionSelector", new Variant(storedRegionId));
        }

        public static void showTargetResults(Target target, System.Drawing.Size imgSz, double resScale)
        {
            Variant var = new Variant();
            target.getParam(cx_3d_target_param_t.CX_3D_PARAM_TARGET_LABEL_MASK, var);
            Mat labelMask = new Mat(), labelMaskColor = new Mat();
            byte[] labelMaskArray = { };
            cx_status_t status = var.get(ref labelMaskArray);
            labelMask.Create(imgSz.Height, imgSz.Width, DepthType.Cv8U, 1);
            labelMask.SetTo(labelMaskArray);
            CvInvoke.Normalize(labelMask, labelMask, 0, 255, NormType.MinMax);
            CvInvoke.ApplyColorMap(labelMask, labelMaskColor, ColorMapType.Jet);
            CvInvoke.NamedWindow("Label Mask", WindowFlags.Normal);
            CvInvoke.Imshow("Label Mask", labelMaskColor);

            target.getParam(cx_3d_target_param_t.CX_3D_PARAM_TARGET_RES, var);
            Mat residuen = new Mat(), residuenColor = new Mat();
            double[] residuenArray = { };
            var.get(ref residuenArray);
            residuen.Create(imgSz.Height, imgSz.Width, DepthType.Cv64F, 1);
            residuen.SetTo(residuenArray);
            residuen.ConvertTo(residuen, DepthType.Cv8U, resScale, 128.0);
            CvInvoke.ApplyColorMap(residuen, residuenColor, ColorMapType.Jet);
            CvInvoke.NamedWindow("Residuen", WindowFlags.Normal);
            CvInvoke.Imshow("Residuen", residuenColor);
        }

        public static void PrintInfo(Calib calib, StreamWriter sw)
        {
            Variant val = new Variant();

            sw.WriteLine("-------------------------------------------------------------------------------");
            calib.getParam(cx_3d_calib_param_t.CX_3D_PARAM_NAME, val);
            string name;
            val.get(out name);
            sw.WriteLine("Name:                       " + name);

            calib.getParam(cx_3d_calib_param_t.CX_3D_PARAM_SENSOR_SN, val);
            string sensorSN;
            val.get(out sensorSN);
            sw.WriteLine("Sensor SN:                  " + sensorSN);

            calib.getParam(cx_3d_calib_param_t.CX_3D_PARAM_DATE, val);
            string date;
            val.get(out date);
            sw.WriteLine("Date:                       " + date);

            calib.getParam(cx_3d_calib_param_t.CX_3D_PARAM_CREATOR, val);
            string creator;
            val.get(out creator);
            sw.WriteLine("Creator:                    " + creator);

            calib.getParam(cx_3d_calib_param_t.CX_3D_PARAM_DESCRIPTION, val);
            string descr;
            val.get(out descr);
            sw.WriteLine("Description:                " + descr);

            calib.getParam(cx_3d_calib_param_t.CX_3D_PARAM_MODEL, val);
            int model = 0;
            val.get(ref model);
            sw.WriteLine("Model:                      " + model);

            calib.getParam(cx_3d_calib_param_t.CX_3D_PARAM_TARGET, val);
            int target = 0;
            val.get(ref target);
            sw.WriteLine("Target:                     " + target);

            calib.getParam(cx_3d_calib_param_t.CX_3D_PARAM_RANGE_SCALE, val);
            double rangeScale = 0;
            val.get(ref rangeScale);
            sw.WriteLine("RangeScale:                 " + rangeScale);

            calib.getParam(cx_3d_calib_param_t.CX_3D_PARAM_SY, val);
            double profileStepWidth = 0;
            val.get(ref profileStepWidth);
            sw.WriteLine("ProfileStepWidth Sy:        " + profileStepWidth);

            calib.getParam(cx_3d_calib_param_t.CX_3D_PARAM_SXY, val);
            double Sxy = 0;
            val.get(ref Sxy);
            sw.WriteLine("Skew Sxy:                   " + Sxy * (180 / Math.PI) + "\x00B0");

            calib.getParam(cx_3d_calib_param_t.CX_3D_PARAM_SZY, val);
            double Szy = 0;
            val.get(ref Szy);
            sw.WriteLine("Skew Szy:                   " + Szy * (180 / Math.PI) + "\x00B0");

            calib.getParam(cx_3d_calib_param_t.CX_3D_PARAM_F, val);
            double lensF = 0;
            val.get(ref lensF);
            sw.WriteLine("Lens focal length:          " + lensF);

            calib.getParam(cx_3d_calib_param_t.CX_3D_PARAM_S_SZ, val);
            int sensorWidth = 0;
            int sensorHeight = 0;
            val.get(ref sensorWidth, ref sensorHeight);
            sw.WriteLine("Sensor size:                " + sensorWidth + " x " + sensorHeight);

            calib.getParam(cx_3d_calib_param_t.CX_3D_PARAM_S_PSZ, val);
            double pixelWidth = 0;
            double pixelHeight = 0;
            val.get(ref pixelWidth, ref pixelHeight);
            sw.WriteLine("Pixel size:                 " + pixelWidth + " x " + pixelHeight);

            calib.getParam(cx_3d_calib_param_t.CX_3D_PARAM_CP, val);
            double cp_x = 0;
            double cp_y = 0;
            val.get(ref cp_x, ref cp_y);
            sw.WriteLine("Principal Point:            " + cp_x + " x " + cp_y);

            calib.getParam(cx_3d_calib_param_t.CX_3D_PARAM_S_R, val);
            Point3d sensorR = val;
            sw.WriteLine("Rotation of sensor plane:   " + sensorR.x * (180 / Math.PI) + ", " + sensorR.y * (180 / Math.PI) + ", " + sensorR.z * (180 / Math.PI));

            calib.getParam(cx_3d_calib_param_t.CX_3D_PARAM_FOV, val);
            double[] fovArray = { };
            val.get(ref fovArray);
            sw.WriteLine("FOV (Field of View):        [" + string.Join(", ", fovArray) + "]");

            calib.getParam(cx_3d_calib_param_t.CX_3D_PARAM_P, val);
            double[] PArray = { };
            val.get(ref PArray);
            Mat P = new Mat(3, PArray.Length / 3, DepthType.Cv64F, 1);
            P.SetTo(PArray);
            double[] _P = new double[3];
            P.Row(0).CopyTo(_P);
            sw.WriteLine("Homography matrix (image plane to laser plane transformation):" + sw.NewLine + "[ " + string.Join(", ", _P) + " ]");
            P.Row(1).CopyTo(_P);
            sw.WriteLine("[ " + string.Join(", ", _P) + " ]");
            P.Row(2).CopyTo(_P);
            sw.WriteLine("[ " + string.Join(", ", _P) + " ]");

            calib.getParam(cx_3d_calib_param_t.CX_3D_PARAM_L, val);
            double[] LArray = { };
            val.get(ref LArray);
            sw.WriteLine("Lens correction matrix:     " + string.Join(", ", LArray));

            calib.getParam(cx_3d_calib_param_t.CX_3D_PARAM_C, val);
            double[] CArray = { };
            val.get(ref CArray);
            sw.WriteLine("Correction matrix:          " + string.Join(", ", CArray));

            calib.getParam(cx_3d_calib_param_t.CX_3D_PARAM_R, val);
            Point3d R = val;
            sw.WriteLine("Sensor to world rotation:");
            sw.WriteLine("\tRx(alpha): " + R.x * (180 / Math.PI) + "\u00b0");
            sw.WriteLine("\tRy(beta):  " + R.y * (180 / Math.PI) + "\x00B0");
            sw.WriteLine("\tRz(gamma): " + R.z * (180 / Math.PI) + "\x00B0");

            calib.getParam(cx_3d_calib_param_t.CX_3D_PARAM_T, val);
            Point3d T = val;
            sw.WriteLine("Sensor to world translation:");
            sw.WriteLine("\ttx: " + T.x);
            sw.WriteLine("\tty: " + T.y);
            sw.WriteLine("\ttz: " + T.z);

            calib.getParam(cx_3d_calib_param_t.CX_3D_PARAM_A_STAT, val);
            double[] A_statArray = { };
            val.get(ref A_statArray);
            Mat A_stat = new Mat(4, 3, DepthType.Cv64F, 1);
            A_stat.SetTo(A_statArray);
            Matrix<double> A_stat_tmp = new Matrix<double>(4, 3, A_stat.DataPointer);
            //A_stat.convertTo(Temp_Mat, CV_32F, 1, 0);
            sw.WriteLine("X Accuracy statistic:");
            sw.WriteLine("\tmean: " + A_stat_tmp[0, 0] + sw.NewLine + "\tstd:  " + A_stat_tmp[0, 1] + sw.NewLine + "\tmax:  " + A_stat_tmp[0, 2]);
            sw.WriteLine("Y Accuracy statistic:");
            sw.WriteLine("\tmean: " + A_stat_tmp[1, 0] + sw.NewLine + "\tstd:  " + A_stat_tmp[1, 1] + sw.NewLine + "\tmax:  " + A_stat_tmp[1, 2]);
            sw.WriteLine("Z Accuracy statistic:");
            sw.WriteLine("\tmean: " + A_stat_tmp[2, 0] + sw.NewLine + "\tstd:  " + A_stat_tmp[2, 1] + sw.NewLine + "\tmax:  " + A_stat_tmp[2, 2]);
            sw.WriteLine("Residuals Accuracy statistic:");
            sw.WriteLine("\tmean: " + A_stat_tmp[3, 0] + sw.NewLine + "\tstd:  " + A_stat_tmp[3, 1] + sw.NewLine + "\tmax:  " + A_stat_tmp[3, 2]);

            calib.getParam(cx_3d_calib_param_t.CX_3D_PARAM_L_STAT, val);
            double[] L_statArray = { };
            val.get(ref L_statArray);
            sw.WriteLine("Lens correction error statistics:" + string.Join(", ", L_statArray));
            sw.WriteLine("-------------------------------------------------------------------------------");
            sw.Flush();
        }

        public static void printTargetInfo(Target target, StreamWriter sw)
        {
            Variant val = new Variant();

            sw.WriteLine("-------------------------------------------------------------------------------");
            target.getParam(cx_3d_target_param_t.CX_3D_PARAM_TARGET_NAME, val);
            String name = val;
            //os << "Name: " << name << endl;

            target.getParam(cx_3d_target_param_t.CX_3D_PARAM_TARGET_SN, val);
            String sensorSN = val;
            //os << "Serial Number: " << sensorSN << endl;

            target.getParam(cx_3d_target_param_t.CX_3D_PARAM_TARGET_DESCRIPTION, val);
            String descr = val;
            //os << "Description: " << descr << endl;

            target.getParam(cx_3d_target_param_t.CX_3D_PARAM_TARGET_TYPE, val);
            int targetType = val;
            //os << "Target: " << target << endl;

            target.getParam(cx_3d_target_param_t.CX_3D_PARAM_TARGET_STAT, val);
            double[] statArray = { };
            val.get(ref statArray);
            sw.WriteLine("Statistics:" + string.Join(", ", statArray));
            sw.WriteLine("-------------------------------------------------------------------------------");
            sw.Flush();
        }

        public static void NormalizeMinMax8U(Image _in, Image _out, bool excludeZeros)
        {
            // use OpenCV functions for implementation
            double minR = 0.0;
            double maxR = 0.0;

            Mat in_cv = new Mat((int)_in.height(), (int)_in.width(), Pf2depth((uint)_in.pixelFormat()), Pf2channels((uint)_in.pixelFormat()), _in.data(), (int)_in.linePitch());
            if (excludeZeros)
            {

                Mat validMask = new Mat();
                Mat zeroMat = new Mat(1, 1, DepthType.Cv32F, 1);
                CvInvoke.Compare(in_cv, zeroMat, validMask, CmpType.NotEqual);
                CvInvoke.MinMaxIdx(in_cv, out minR, out maxR, null, null, validMask);
            }
            else
            {
                CvInvoke.MinMaxIdx(in_cv, out minR, out maxR, null, null);
            }
            double s = (maxR == minR) ? 1.0 : 255.0 / (maxR - minR);
            _out.create((uint)in_cv.Rows, (uint)in_cv.Cols, cx_pixel_format.CX_PF_MONO_8);

            Mat out_cv = new Mat((int)_out.height(), (int)_out.width(), Pf2depth((uint)_out.pixelFormat()), Pf2channels((uint)_out.pixelFormat()), _out.data(), (int)_out.linePitch());
            in_cv.ConvertTo(out_cv, DepthType.Cv8U, s, -minR * s);
        }

        public static void ShowPointCloud(Viz3d viz, PointCloud pc, string id, int flags, int colormap)
        {
            // create OpenCV object from point cloud, just reference to buffer
            Mat cloud = new Mat((int)pc.points.height(), (int)pc.points.width(), Pf2depth((uint)pc.points.pixelFormat()), Pf2channels((uint)pc.points.pixelFormat()), pc.points.data(), (int)pc.points.linePitch());
            // cloud.convertTo(cloud, CV_32FC3);
            Mat normals = new Mat((int)pc.normals.height(), (int)pc.normals.width(), Pf2depth((uint)pc.normals.pixelFormat()), Pf2channels((uint)pc.normals.pixelFormat()), pc.normals.data(), (int)pc.normals.linePitch());

            Mat colors = new Mat((int)pc.colors.height(), (int)pc.colors.width(), Pf2depth((uint)pc.colors.pixelFormat()), Pf2channels((uint)pc.colors.pixelFormat()), pc.colors.data(), (int)pc.colors.linePitch());
            if (colormap > 0)
            {
                Mat colors_map = new Mat();
                CvInvoke.ApplyColorMap(colors, colors_map, (ColorMapType)colormap - 1); // cv colormap definition starts from 0, we use 0 for grayscale
                CvInvoke.Swap(colors, colors_map);
            }

            viz.SetBackgroundMeshLab();
            viz.ShowWidget(id + "_cs", new WCoordinateSystem(10.0));
            if (flags == 2)
            {
                throw new NotImplementedException("EmguCV doesn't implement the WCloud's constructor with normals.");
                //viz.ShowWidget(id, new WCloud(cloud, colors, normals));	// show with colors and normals
            }
            else if (flags == 1)
            {
                viz.ShowWidget(id, new WCloud(cloud, colors));  // show without normals
            }
            else
            {
                Rgb ColorWhite = new Rgb(System.Drawing.Color.White);
                viz.ShowWidget(id, new WCloud(cloud, ColorWhite.MCvScalar)); //  show without colors and normals
            }
            //viz.SetRenderingProperty(id, cv::viz::SHADING, cv::viz::SHADING_PHONG);
        }
    }
}
