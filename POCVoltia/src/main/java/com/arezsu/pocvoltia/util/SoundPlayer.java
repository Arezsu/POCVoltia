package com.arezsu.pocvoltia.util;

import javax.sound.sampled.AudioInputStream;
import javax.sound.sampled.AudioSystem;
import javax.sound.sampled.Clip;
import java.net.URL;

public class SoundPlayer {
    
    public static void playSound(String soundFileName) {
        try {
            URL soundURL = SoundPlayer.class.getResource("/" + soundFileName);
            if (soundURL != null) {
                AudioInputStream audioIn = AudioSystem.getAudioInputStream(soundURL);
                Clip clip = AudioSystem.getClip();
                clip.open(audioIn);
                clip.start();
            } else {
                System.err.println("No se encontró el archivo de sonido: /" + soundFileName);
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}