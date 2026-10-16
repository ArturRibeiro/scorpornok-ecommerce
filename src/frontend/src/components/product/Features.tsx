import { Card, CardContent } from "@/components/ui/card";
import { RotateCcw, Shield, Truck } from "lucide-react";

export default function Features() {
  const features = [
    { icon: Truck, title: "Frete grátis", desc: "Em compras acima de R$ 50" },
    { icon: Shield, title: "Garantia", desc: "1 ano de garantia" },
    { icon: RotateCcw, title: "Troca fácil", desc: "30 dias para trocar ou devolver" },
  ];
  return (
    <Card className="mb-16">
      <CardContent className="p-8">
        <div className="grid md:grid-cols-3 gap-8">
          {features.map((feature, index) => (
            <div key={index} className="flex items-start gap-4">
              <div className="p-3 bg-primary/10 rounded-lg">
                <feature.icon className="h-6 w-6 text-primary" />
              </div>
              <div>
                <h2 className="font-semibold text-foreground mb-1">
                  {feature.title}
                </h2>
                <p className="text-sm text-muted-foreground">{feature.desc}</p>
              </div>
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}
