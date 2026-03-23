<?php

namespace Database\Seeders;

use App\Models\Component;
use App\Models\WattageList;
use Illuminate\Database\Seeder;

class WattageListSeeder extends Seeder
{
    public function run(): void
    {
        // tray_id => wattage (power draw in watts; PSU and Case = 0)
        $wattages = [
            // CPU
            1  => 105,
            2  => 120,
            3  => 125,
            4  => 253,
            // Motherboard
            5  => 35,
            6  => 30,
            7  => 40,
            8  => 28,
            // RAM
            9  => 5,
            10 => 5,
            11 => 4,
            12 => 4,
            // GPU
            13 => 115,
            14 => 200,
            15 => 263,
            16 => 450,
            // Storage
            17 => 5,
            18 => 7,
            19 => 8,
            // PSU (provides power, does not draw net power)
            20 => 0,
            21 => 0,
            22 => 0,
            23 => 0,
            // Case
            24 => 0,
            25 => 0,
            26 => 0,
            27 => 0,
            // Cooling
            28 => 2,
            29 => 2,
            30 => 10,
            31 => 2,
        ];

        foreach ($wattages as $trayId => $wattage) {
            $component = Component::where('tray_id', $trayId)->firstOrFail();
            WattageList::create([
                'component_id' => $component->id,
                'wattage'      => $wattage,
            ]);
        }
    }
}
