<?php

namespace Database\Seeders;

use App\Models\Component;
use App\Models\Requirement;
use Illuminate\Database\Seeder;

class RequirementSeeder extends Seeder
{
    public function run(): void
    {
        // tray_id => [ [name, value], ... ]
        // name = what kind of requirement, value = what it must match
        $requirements = [
            // CPUs require a motherboard with matching socket
            1 => [['name' => 'Socket', 'value' => 'AM5']],
            2 => [['name' => 'Socket', 'value' => 'AM5']],
            3 => [['name' => 'Socket', 'value' => 'LGA1700']],
            4 => [['name' => 'Socket', 'value' => 'LGA1700']],

            // Motherboards require matching RAM type
            5 => [['name' => 'RAM Type', 'value' => 'DDR5']],
            6 => [['name' => 'RAM Type', 'value' => 'DDR5']],
            7 => [['name' => 'RAM Type', 'value' => 'DDR5']],
            // tray_id 8 (B760M DS3H) supports both DDR4 and DDR5 — no strict requirement

            // RAM requires motherboard with matching RAM type
            9  => [['name' => 'RAM Type', 'value' => 'DDR5']],
            10 => [['name' => 'RAM Type', 'value' => 'DDR5']],
            11 => [['name' => 'RAM Type', 'value' => 'DDR4']],
            12 => [['name' => 'RAM Type', 'value' => 'DDR4']],

            // GPUs, Storage, PSUs have no compatibility requirements in this schema

            // Cases require a motherboard with matching form factor
            24 => [
                ['name' => 'Form Factor', 'value' => 'ATX'],
                ['name' => 'Form Factor', 'value' => 'mATX'],
            ],
            25 => [['name' => 'Form Factor', 'value' => 'ATX']],
            26 => [
                ['name' => 'Form Factor', 'value' => 'ATX'],
                ['name' => 'Form Factor', 'value' => 'mATX'],
            ],
            27 => [
                ['name' => 'Form Factor', 'value' => 'mATX'],
                ['name' => 'Form Factor', 'value' => 'ITX'],
            ],

            // Coolers require a CPU with a supported socket
            28 => [
                ['name' => 'Socket', 'value' => 'AM4'],
                ['name' => 'Socket', 'value' => 'AM5'],
                ['name' => 'Socket', 'value' => 'LGA1700'],
            ],
            29 => [
                ['name' => 'Socket', 'value' => 'AM4'],
                ['name' => 'Socket', 'value' => 'AM5'],
                ['name' => 'Socket', 'value' => 'LGA1700'],
            ],
            30 => [
                ['name' => 'Socket', 'value' => 'AM4'],
                ['name' => 'Socket', 'value' => 'AM5'],
                ['name' => 'Socket', 'value' => 'LGA1700'],
            ],
            31 => [
                ['name' => 'Socket', 'value' => 'AM4'],
                ['name' => 'Socket', 'value' => 'AM5'],
                ['name' => 'Socket', 'value' => 'LGA1700'],
            ],
        ];

        foreach ($requirements as $trayId => $rows) {
            $component = Component::where('tray_id', $trayId)->firstOrFail();
            foreach ($rows as $row) {
                Requirement::create([
                    'component_id' => $component->id,
                    'name'         => $row['name'],
                    'value'        => $row['value'],
                ]);
            }
        }
    }
}
