<?php

namespace Tests\Unit;

use App\Models\Component;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Tests\TestCase;

class ComponentCatalogTest extends TestCase
{
    use RefreshDatabase;

    public function test_seeded_components_include_relationship_data_needed_for_compatibility_checks(): void
    {
        $this->seed();

        $cpu = Component::with(['category', 'specifications', 'wattageLists', 'requirements'])
            ->where('tray_id', 1)
            ->firstOrFail();

        $this->assertSame('CPU', $cpu->category->name);
        $this->assertTrue($cpu->specifications->contains('name', 'Socket'));
        $this->assertTrue($cpu->specifications->contains('name', 'TDP'));
        $this->assertSame(105, $cpu->wattageLists->first()->wattage);
        $this->assertTrue($cpu->requirements->contains(
            fn ($requirement) => $requirement->name === 'Socket' && $requirement->value === 'AM5'
        ));
    }

    public function test_seeded_catalog_contains_multiple_form_factor_and_socket_options(): void
    {
        $this->seed();

        $case = Component::with('specifications')->where('tray_id', 24)->firstOrFail();
        $cooler = Component::with(['specifications', 'requirements'])->where('tray_id', 30)->firstOrFail();

        $caseFormFactors = $case->specifications
            ->where('name', 'Form Factor')
            ->pluck('value')
            ->all();

        $coolerSockets = $cooler->requirements
            ->where('name', 'Socket')
            ->pluck('value')
            ->all();

        $this->assertEqualsCanonicalizing(['ATX', 'mATX'], $caseFormFactors);
        $this->assertEqualsCanonicalizing(['AM4', 'AM5', 'LGA1700'], $coolerSockets);
    }
}
