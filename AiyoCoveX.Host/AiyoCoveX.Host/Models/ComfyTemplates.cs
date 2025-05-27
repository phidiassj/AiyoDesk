namespace AiyoCoveX.Host.Models
{
    public class ComfyTemplates
    {
        
        public static string ReplacePromptString = @"<<<###ReplacePromptHere###>>>";

        public static string NitroVibrant = """
                        {
              "1": {
                "inputs": {
                  "ckpt_name": "nitrosd-vibrant_comfyui.safetensors"
                },
                "class_type": "CheckpointLoaderSimple",
                "_meta": {
                  "title": "Load Checkpoint"
                }
              },
              "2": {
                "inputs": {
                  "shifted_timestep": 500,
                  "model": [
                    "1",
                    0
                  ]
                },
                "class_type": "Timestep Shift Model",
                "_meta": {
                  "title": "Timestep Shift Model"
                }
              },
              "4": {
                "inputs": {
                  "text": "<<<###ReplacePromptHere###>>>",
                  "clip": [
                    "1",
                    1
                  ]
                },
                "class_type": "CLIPTextEncode",
                "_meta": {
                  "title": "CLIP Text Encode (Prompt)"
                }
              },
              "5": {
                "inputs": {
                  "text": "",
                  "clip": [
                    "1",
                    1
                  ]
                },
                "class_type": "CLIPTextEncode",
                "_meta": {
                  "title": "CLIP Text Encode (Prompt)"
                }
              },
              "6": {
                "inputs": {
                  "width": 1024,
                  "height": 1024,
                  "batch_size": 1
                },
                "class_type": "EmptyLatentImage",
                "_meta": {
                  "title": "Empty Latent Image"
                }
              },
              "7": {
                "inputs": {
                  "seed": 2992032114344,
                  "steps": 1,
                  "cfg": 1,
                  "sampler_name": "lcm",
                  "scheduler": "simple",
                  "denoise": 1,
                  "model": [
                    "2",
                    0
                  ],
                  "positive": [
                    "4",
                    0
                  ],
                  "negative": [
                    "5",
                    0
                  ],
                  "latent_image": [
                    "6",
                    0
                  ]
                },
                "class_type": "KSampler",
                "_meta": {
                  "title": "KSampler"
                }
              },
              "8": {
                "inputs": {
                  "samples": [
                    "7",
                    0
                  ],
                  "vae": [
                    "1",
                    2
                  ]
                },
                "class_type": "VAEDecode",
                "_meta": {
                  "title": "VAE Decode"
                }
              },
              "9": {
                "inputs": {
                  "filename_prefix": "ComfyUI",
                  "images": [
                    "8",
                    0
                  ]
                },
                "class_type": "SaveImage",
                "_meta": {
                  "title": "Save Image"
                }
              }
            }
            """;

    }
}
